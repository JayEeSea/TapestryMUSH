#include <iostream>
#include <thread>
#include <vector>
#include <cstring>
#include <netinet/in.h>
#include <unistd.h>

#define PORT 4000
#define MAX_CLIENTS 10

std::vector<int> clients;

void handle_client(int client_socket) {
    char buffer[1024];
    std::string welcome_msg = "Welcome to the MUSH server!\nPlease enter your name: ";
    send(client_socket, welcome_msg.c_str(), welcome_msg.size(), 0);
    
    while (true) {
        memset(buffer, 0, sizeof(buffer));
        int bytes_received = recv(client_socket, buffer, sizeof(buffer) - 1, 0);
        if (bytes_received <= 0) {
            std::cout << "Client disconnected." << std::endl;
            close(client_socket);
            return;
        }
        
        std::string input(buffer);
        input.erase(input.find_last_not_of("\r\n") + 1); // Trim newlines
        std::cout << "Client: " << input << std::endl;
        
        if (input == "quit") {
            std::string goodbye = "Goodbye!\n";
            send(client_socket, goodbye.c_str(), goodbye.size(), 0);
            close(client_socket);
            return;
        }
        
        std::string response = "You said: " + input + "\n";
        send(client_socket, response.c_str(), response.size(), 0);
    }
}

int main() {
    int server_socket;
    struct sockaddr_in server_addr;
    
    server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        std::cerr << "Error creating socket." << std::endl;
        return -1;
    }
    
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(PORT);
    
    if (bind(server_socket, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        std::cerr << "Binding failed." << std::endl;
        return -1;
    }
    
    if (listen(server_socket, MAX_CLIENTS) < 0) {
        std::cerr << "Error in listen." << std::endl;
        return -1;
    }
    
    std::cout << "MUSH server listening on port " << PORT << "..." << std::endl;
    
    while (true) {
        int client_socket;
        struct sockaddr_in client_addr;
        socklen_t client_size = sizeof(client_addr);
        client_socket = accept(server_socket, (struct sockaddr*)&client_addr, &client_size);
        
        if (client_socket < 0) {
            std::cerr << "Error accepting client connection." << std::endl;
            continue;
        }
        
        std::cout << "New client connected." << std::endl;
        clients.push_back(client_socket);
        std::thread(handle_client, client_socket).detach();
    }
    
    close(server_socket);
    return 0;
}