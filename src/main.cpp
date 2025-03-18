#include <iostream>
#include <fstream>
#include <sstream>
#include <map>
#include <string>
#include <thread>
#include <vector>
#include <cstring>
#include <netinet/in.h>
#include <unistd.h>
#include <algorithm>
#include <unistd.h>
#include <limits.h>
#include <sys/stat.h>
#include <sys/types.h>

#define DEFAULT_PORT 4000
#define DEFAULT_MAX_CLIENTS 10
#define DEFAULT_LOG_DIRECTORY "../logs"

std::map<std::string, std::string> config;
std::ofstream log_file;

// Enum for log levels
enum LogLevel { DEBUG, INFO, WARNING, ERROR };
LogLevel current_log_level = INFO; // Default log level

// Convert log level to string
std::string log_level_to_string(LogLevel level) {
    switch (level) {
        case DEBUG: return "DEBUG";
        case INFO: return "INFO";
        case WARNING: return "WARNING";
        case ERROR: return "ERROR";
        default: return "UNKNOWN";
    }
}

// Convert string to LogLevel
LogLevel string_to_log_level(const std::string& level) {
    if (level == "DEBUG") return DEBUG;
    if (level == "INFO") return INFO;
    if (level == "WARNING") return WARNING;
    if (level == "ERROR") return ERROR;
    return INFO; // Default if invalid
}

// Logging function
void log(LogLevel level, const std::string& message) {
    if (level < current_log_level) return; // Ignore messages below the set log level
    
    std::time_t now = std::time(nullptr);
    std::string log_entry = "[" + std::string(std::ctime(&now)) + "] [" + log_level_to_string(level) + "] " + message;
    log_entry.erase(std::remove(log_entry.begin(), log_entry.end(), '\n'), log_entry.end()); // Remove newline from ctime
    
    std::cout << log_entry << std::endl;
    if (log_file.is_open()) {
        log_file << log_entry << std::endl;
    }
}

void debug_print_config() {
    log(DEBUG, "Current Config Values:");
    for (const auto& pair : config) {
        log(DEBUG, pair.first + " = " + pair.second);
    }
}

// Function to load config file
void load_config(const std::string& filename) {
    std::ifstream file(filename);
    if (!file) {
        log(WARNING, "Config file not found, using defaults.");
        return;
    }

    std::string line;
    while (std::getline(file, line)) {
        // Ignore comments, empty lines, and section headers (e.g., [server], [logging])
        if (line.empty() || line[0] == '#' || line[0] == ';' || line[0] == '[') 
            continue;

        std::istringstream is_line(line);
        std::string key, value;
        if (std::getline(is_line, key, '=') && std::getline(is_line, value)) {
            // Trim spaces and newlines
            key.erase(std::remove_if(key.begin(), key.end(), ::isspace), key.end());
            value.erase(std::remove_if(value.begin(), value.end(), ::isspace), value.end());

            // Store key-value pairs in config map
            config[key] = value;
            log(DEBUG, "Loaded config key [" + key + "] with value [" + value + "]");
        }
    }
}

// Detect correct directory for tapestry.cnf dynamically
void load_config_from_root() {
    char path[PATH_MAX];
    if (getcwd(path, sizeof(path))) {
        std::string full_path = std::string(path) + "/../tapestry.cnf";
        load_config(full_path);
    } else {
        log(ERROR, "Failed to determine current directory.");
    }
}

// Function to get config values with defaults
std::string get_config_value(const std::string& key, const std::string& default_value) {
    if (config.count(key)) {
        return config[key];
    } else {
        log(WARNING, "Config key '" + key + "' not found, using default value.");
        return default_value;
    }
}

// Function to initialize log file based on config
void initialize_log_file() {
    std::string log_directory = get_config_value("log_directory", DEFAULT_LOG_DIRECTORY);
    
    // Check if the directory exists
    if (access(log_directory.c_str(), F_OK) != 0) {
        // Try to create the directory
        if (mkdir(log_directory.c_str(), 0777) != 0) {
            std::cerr << "Failed to create log directory: " << log_directory << std::endl;
            return;
        }
    }
    
    std::string log_path = log_directory + "/server.log";
    log_file.open(log_path, std::ios::app);
    if (!log_file.is_open()) {
        std::cerr << "Failed to open log file at " << log_path << std::endl;
    }
}

// Main server function
void start_server(int port, int max_clients) {
    int server_socket;
    struct sockaddr_in server_addr;

    server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        log(ERROR, "Error creating socket.");
        return;
    }

    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(port);

    if (bind(server_socket, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        log(ERROR, "Binding failed.");
        return;
    }

    if (listen(server_socket, max_clients) < 0) {
        log(ERROR, "Error in listen.");
        return;
    }

    log(INFO, "MUSH server '" + get_config_value("name", "TapestryMUSH") + "' listening on port " + std::to_string(port) + "...");

    while (true) {
        int client_socket;
        struct sockaddr_in client_addr;
        socklen_t client_size = sizeof(client_addr);
        client_socket = accept(server_socket, (struct sockaddr*)&client_addr, &client_size);

        if (client_socket < 0) {
            log(ERROR, "Error accepting client connection.");
            continue;
        }

        log(INFO, "New client connected.");
        std::string welcome_msg = "Welcome to " + get_config_value("name", "TapestryMUSH") + "!\n";
        send(client_socket, welcome_msg.c_str(), welcome_msg.size(), 0);
        close(client_socket);
    }

    close(server_socket);
}

int main() {
    load_config_from_root();
    initialize_log_file();
    debug_print_config();  // Debug output of all loaded config keys

    current_log_level = string_to_log_level(get_config_value("log_level", "INFO"));
    log(INFO, "Log level set to " + log_level_to_string(current_log_level));

    int port = std::stoi(get_config_value("port", std::to_string(DEFAULT_PORT)));
    int max_clients = std::stoi(get_config_value("max_clients", std::to_string(DEFAULT_MAX_CLIENTS)));

    start_server(port, max_clients);
    return 0;
}