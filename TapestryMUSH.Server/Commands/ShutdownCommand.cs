using TapestryMUSH.Core.Session;
using Microsoft.Extensions.Hosting;
using TapestryMUSH.Core.Commands;

namespace TapestryMUSH.Server.Commands;

public class ShutdownCommand : ICommand
{
    public string Name => "@shutdown";
    public IEnumerable<string> Aliases => Array.Empty<string>();

    private readonly IHostApplicationLifetime _lifetime;

    public ShutdownCommand(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);

        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        if (!player.Flags.Contains("WIZARD"))
        {
            await session.SendLineAsync("Permission denied.");
            return;
        }

        await session.SendLineAsync("Shutting down server...");
        Console.WriteLine($"[SHUTDOWN] Initiated by {player.Username} [#{player.Id}]");

        await Task.Delay(1000);
        Environment.Exit(0);
    }
}
