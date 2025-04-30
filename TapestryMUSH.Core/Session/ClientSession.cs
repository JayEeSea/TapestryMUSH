using System.Net.Sockets;
using System.Text;
using TapestryMUSH.Core.Commands.CommandRegistry;
using TapestryMUSH.Core.Services;
using TapestryMUSH.Core.Session;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Session;

public class ClientSession
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly byte[] _buffer = new byte[1024];
    private readonly CommandRouter _commandRouter;
    private readonly AccountService _accountService;
    public AccountService AccountService => _accountService;
    public CommandRouter CommandRouter => _commandRouter;
    public int? PlayerId { get; set; }
    public Player? CurrentPlayer { get; set; }

    public ClientSession(TcpClient client, AccountService accountService, CommandRouter commandRouter)
    {
        _client = client;
        _accountService = accountService;
        _commandRouter = commandRouter;
        _stream = client.GetStream();
    }

    public async Task RunAsync()
    {
        // To change to a file reference later
        await SendLineAsync("Welcome to TapestryMUSH!");
        await SendLineAsync("Type 'connect <username> <password>' or 'create <username> <password>' or 'quit'.");

        while (_client.Connected)
        {
            var bytesRead = await _stream.ReadAsync(_buffer);
            if (bytesRead == 0) break;

            var input = Encoding.UTF8.GetString(_buffer, 0, bytesRead).Trim();
            if (string.IsNullOrWhiteSpace(input)) continue;

            await HandleCommandAsync(input);
        }

        _client.Close();
    }

    private async Task HandleCommandAsync(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return;

        var rawCommand = parts[0];
        var command = rawCommand.ToLower();

        switch (command)
        {
            case "quit":
                await SendLineAsync("Goodbye!");
                _client.Close();
                break;

            case "create":
                if (parts.Length < 4)
                {
                    await SendLineAsync("Usage: create <username> <password> <email>");
                    break;
                }

                var createResponse = await _accountService.CreateAccountAsync(parts[1], parts[2], parts[3]);
                await SendLineAsync(createResponse);
                break;

            case "connect":
                if (parts.Length < 3)
                {
                    await SendLineAsync("Usage: connect <username> <password>");
                    break;
                }

                var connectedPlayer = await _accountService.ConnectAsync(parts[1], parts[2]);

                if (connectedPlayer == null)
                {
                    await SendLineAsync("Invalid username or password.");
                    break;
                }

                PlayerId = connectedPlayer.Id;
                CurrentPlayer = connectedPlayer;

                await SendLineAsync($"Welcome back, {connectedPlayer.Username}!");
                await _commandRouter.RouteAsync(this, "look");
                break;

            case "logout":
                if (rawCommand == "LOGOUT")
                {
                    PlayerId = null;
                    CurrentPlayer = null;

                    await SendLineAsync("You have been logged out.");
                    await SendLineAsync("Type 'connect <username> <password>' or 'create <username> <password>' or 'quit'.");
                }
                else
                {
                    await SendLineAsync("Unknown command. (Try HELP?)");
                }
                break;

            default:
                await _commandRouter.RouteAsync(this, input);
                break;
        }
    }



    public async Task SendLineAsync(string message)
    {
        var data = Encoding.UTF8.GetBytes(message + "\r\n");
        await _stream.WriteAsync(data);
    }
}
