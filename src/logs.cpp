#include "logs.h"
#include "constants.h"
#include "config.h"
#include <iostream>
#include <fstream>
#include <filesystem>
#include <ctime>
#include <algorithm>
#include <mutex> // Include for std::mutex

// Define log_file here
std::ofstream log_file;

// Enum for log levels
enum LogLevel { DEBUG, INFO, WARNING, ERROR };

std::mutex log_mutex; // Global mutex for logging

namespace Logging {
    LogLevel current_log_level = INFO;

    std::string log_level_to_string(LogLevel level) {
        switch (level) {
            case DEBUG: return "DEBUG";
            case INFO: return "INFO";
            case WARNING: return "WARNING";
            case ERROR: return "ERROR";
            default: return "UNKNOWN";
        }
    }

    LogLevel string_to_log_level(const std::string& level) {
        if (level == "DEBUG") return DEBUG;
        if (level == "INFO") return INFO;
        if (level == "WARNING") return WARNING;
        if (level == "ERROR") return ERROR;
        return INFO;
    }

    void log(LogLevel level, const std::string& message) {
        if (level < current_log_level) return;

        std::time_t now = std::time(nullptr);
        std::string log_entry = "[" + std::string(std::ctime(&now)) + "] [" + log_level_to_string(level) + "] " + message;
        log_entry.erase(std::remove(log_entry.begin(), log_entry.end(), '\n'), log_entry.end());

        std::lock_guard<std::mutex> lock(log_mutex);

        std::cout << log_entry << std::endl;
        if (log_file.is_open()) {
            log_file << log_entry << std::endl;
        }
    }

    void initialize_log_file() {
        std::string log_directory = get_config_value("log_directory", std::string(DEFAULT_LOG_DIRECTORY));

        std::filesystem::path log_dir_path(log_directory);
        if (!std::filesystem::exists(log_dir_path)) {
            try {
                std::filesystem::create_directories(log_dir_path);
            } catch (const std::filesystem::filesystem_error& e) {
                std::cerr << "Failed to create log directory: " << e.what() << std::endl;
                return;
            }
        }

        std::string log_path = log_directory + "/server.log";
        log_file.open(log_path, std::ios::app);
        if (!log_file.is_open()) {
            std::cerr << "Failed to open log file at " << log_path << std::endl;
        }
    }
}

// Debug function to print all loaded configuration values
void debug_print_config() {
    Logging::log(DEBUG, "Current Config Values:");
    for (const auto& pair : config) {
        Logging::log(DEBUG, pair.first + " = " + pair.second);
    }
}