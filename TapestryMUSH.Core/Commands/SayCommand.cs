using TapestryMUSH.Core.Session;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Commands;

public class SayCommand : ICommand
{
    public string Name => "say";
    public IEnumerable<string> Aliases => new[] { "\"" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player?.CurrentRoom == null)
        {
            await session.SendLineAsync("You are nowhere.");
            return;
        }

        // Strip the command name or quote prefix
        string message = input.StartsWith("\"")
            ? input.Substring(1).Trim()
            : input.Substring(input.IndexOf(' ') + 1).Trim();

        await session.SendLineAsync($"You say, \"{message}\"");

        // Later: broadcast to other players in room
    }
}