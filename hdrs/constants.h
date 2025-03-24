#pragma once

#include <string_view>

// Default configuration values
constexpr int DEFAULT_PORT = 4000;
constexpr int DEFAULT_MAX_CLIENTS = 10;
constexpr std::string_view DEFAULT_LOG_DIRECTORY = "../logs";
constexpr std::string_view DEFAULT_CNF_FILE = "../game/tapestry.cnf";

// Global configuration map to store key-value pairs from the config file
std::map<std::string, std::string> config;