#include "constants.h"
#include <netinet/in.h>

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