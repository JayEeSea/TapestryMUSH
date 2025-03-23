#include logs.h
#include <iostream>
#include <fstream>

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