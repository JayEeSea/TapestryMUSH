#pragma once

#include <string>
#include <fstream> // Include for std::ofstream

// Declare log_file as an external variable
extern std::ofstream log_file;

namespace Logging {
    enum LogLevel {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    };

    void log(LogLevel level, const std::string& message);
    LogLevel string_to_log_level(const std::string& level);
    std::string log_level_to_string(LogLevel level);
    void initialize_log_file();
}