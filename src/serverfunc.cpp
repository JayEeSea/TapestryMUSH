#include serverfunc.h

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