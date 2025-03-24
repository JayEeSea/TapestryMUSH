#include "constants.h"
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