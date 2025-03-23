#include main.h
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

// Global configuration map to store key-value pairs from the config file
std::map<std::string, std::string> config;

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
        std::string full_path = std::string(path) + DEFAULT_CNF_FILE;
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