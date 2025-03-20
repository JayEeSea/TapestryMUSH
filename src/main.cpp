#include <iostream>
#include <fstream>
#include <sstream>
#include <map>
#include <string>
#include <thread>
#include <vector>
#include <cstring>
#include <algorithm>
#include <ctime>
#include <filesystem>
#include <csignal>
#include <netinet/in.h>
#include <unistd.h>
#include <limits.h>
#include <sys/stat.h>
#include <sys/types.h>

// Default configuration values
#define DEFAULT_PORT 4000
#define DEFAULT_MAX_CLIENTS 10
#define DEFAULT_LOG_DIRECTORY "../logs"

// Global configuration map to store key-value pairs from the config file
std::map<std::string, std::string> config;

// Global log file stream
std::ofstream log_file;

// Enum for log levels
enum LogLevel { DEBUG, INFO, WARNING, ERROR };
LogLevel current_log_level = INFO; // Default log level

// Convert log level enum to string for logging purposes
std::string log_level_to_string(LogLevel level) {
    switch (level) {
        case DEBUG: return "DEBUG";
        case INFO: return "INFO";
        case WARNING: return "WARNING";
        case ERROR: return "ERROR";
        default: return "UNKNOWN";
    }
}

// Convert string to LogLevel enum
LogLevel string_to_log_level(const std::string& level) {
    if (level == "DEBUG") return DEBUG;
    if (level == "INFO") return INFO;
    if (level == "WARNING") return WARNING;
    if (level == "ERROR") return ERROR;
    return INFO; // Default to INFO if the string is invalid
}

// Logging function to log messages to both console and log file
void log(LogLevel level, const std::string& message) {
    if (level < current_log_level) return; // Ignore messages below the current log level
    
    // Get the current time for the log entry
    std::time_t now = std::time(nullptr);
    std::string log_entry = "[" + std::string(std::ctime(&now)) + "] [" + log_level_to_string(level) + "] " + message;
    log_entry.erase(std::remove(log_entry.begin(), log_entry.end(), '\n'), log_entry.end()); // Remove newline from ctime
    
    // Print log entry to console
    std::cout << log_entry << std::endl;

    // Write log entry to file if the log file is open
    if (log_file.is_open()) {
        log_file << log_entry << std::endl;
    }
}

// Debug function to print all loaded configuration values
void debug_print_config() {
    log(DEBUG, "Current Config Values:");
    for (const auto& pair : config) {
        log(DEBUG, pair.first + " = " + pair.second);
    }
}

// Function to load configuration from a file
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

        // Parse key-value pairs separated by '='
        std::istringstream is_line(line);
        std::string key, value;
        if (std::getline(is_line, key, '=') && std::getline(is_line, value)) {
            // Remove whitespace from key and value
            key.erase(std::remove_if(key.begin(), key.end(), ::isspace), key.end());
            value.erase(std::remove_if(value.begin(), value.end(), ::isspace), value.end());
            if (!key.empty() && !value.empty()) {
                config[key] = value; // Store the key-value pair in the config map
                log(DEBUG, "Loaded config key [" + key + "] with value [" + value + "]");
            } else {
                log(WARNING, "Malformed config line: " + line);
            }
        }
    }
}

// Function to detect the correct directory for the configuration file dynamically
void load_config_from_root() {
    char path[PATH_MAX];
    if (getcwd(path, sizeof(path))) {
        std::string full_path = std::string(path) + "/../tapestry.cnf";
        load_config(full_path); // Load the configuration file
    } else {
        log(ERROR, "Failed to determine current directory.");
    }
}

// Function to retrieve a configuration value with a default fallback
std::string get_config_value(const std::string& key, const std::string& default_value) {
    if (config.count(key)) {
        return config[key]; // Return the value if the key exists
    } else {
        log(WARNING, "Config key '" + key + "' not found, using default value.");
        return default_value; // Return the default value if the key is not found
    }
}

// Function to initialize the log file based on the configuration
void initialize_log_file() {
    std::string log_directory = get_config_value("log_directory", DEFAULT_LOG_DIRECTORY);

    // Use std::filesystem to check and create the directory
    std::filesystem::path log_dir_path(log_directory);
    if (!std::filesystem::exists(log_dir_path)) {
        try {
            std::filesystem::create_directories(log_dir_path); // Create the directory if it doesn't exist
        } catch (const std::filesystem::filesystem_error& e) {
            std::cerr << "Failed to create log directory: " << e.what() << std::endl;
            return;
        }
    }

    // Open the log file in append mode
    std::string log_path = log_directory + "/server.log";
    log_file.open(log_path, std::ios::app);
    if (!log_file.is_open()) {
        std::cerr << "Failed to open log file at " << log_path << std::endl;
    }
}

// Global flag to indicate whether the server is running
bool running = true;

// Signal handler to gracefully shut down the server
void signal_handler(int signal) {
    log(INFO, "Signal received, shutting down server...");
    running = false;
}

// Function to handle a single client connection
void handle_client(int client_socket) {
    log(INFO, "New client connected.");
    std::string welcome_msg = "Welcome to " + get_config_value("name", "TapestryMUSH") + "!\n";
    send(client_socket, welcome_msg.c_str(), welcome_msg.size(), 0); // Send a welcome message to the client
    close(client_socket); // Close the client socket
}

// Main server function to start listening for client connections
void start_server(int port, int max_clients) {
    int server_socket;
    struct sockaddr_in server_addr;

    // Create a socket
    server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        log(ERROR, "Error creating socket.");
        return;
    }

    // Configure the server address
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(port);

    // Bind the socket to the address
    if (bind(server_socket, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        log(ERROR, "Binding failed.");
        return;
    }

    // Start listening for incoming connections
    if (listen(server_socket, max_clients) < 0) {
        log(ERROR, "Error in listen.");
        return;
    }

    log(INFO, "MUSH server '" + get_config_value("name", "TapestryMUSH") + "' listening on port " + std::to_string(port) + "...");

    // Main server loop to accept and handle client connections
    while (running) {
        int client_socket;
        struct sockaddr_in client_addr;
        socklen_t client_size = sizeof(client_addr);
        client_socket = accept(server_socket, (struct sockaddr*)&client_addr, &client_size);

        if (client_socket < 0) {
            if (running) {
                log(ERROR, "Error accepting client connection.");
            }
            continue;
        }

        // Handle the client connection in a separate thread
        std::thread client_thread(handle_client, client_socket);
        client_thread.detach(); // Detach the thread to handle the client independently
    }

    close(server_socket); // Close the server socket when shutting down
}

// Main entry point of the program
int main() {
    // Register signal handlers for graceful shutdown
    std::signal(SIGINT, signal_handler); // Handle Ctrl+C
    std::signal(SIGTERM, signal_handler); // Handle termination signals

    // Load configuration and initialize the server
    load_config_from_root();
    initialize_log_file();
    debug_print_config();  // Debug output of all loaded config keys

    // Set the log level based on the configuration
    current_log_level = string_to_log_level(get_config_value("log_level", "INFO"));
    log(INFO, "Log level set to " + log_level_to_string(current_log_level));

    // Retrieve server port and max clients from the configuration
    int port = std::stoi(get_config_value("port", std::to_string(DEFAULT_PORT)));
    int max_clients = std::stoi(get_config_value("max_clients", std::to_string(DEFAULT_MAX_CLIENTS)));

    // Start the server
    start_server(port, max_clients);
    return 0;
}