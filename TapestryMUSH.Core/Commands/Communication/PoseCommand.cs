using TapestryMUSH.Core.Session;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Commands.Communication;

public class PoseCommand : ICommand
{
    public string Name => "pose";
    public IEnumerable<string> Aliases => new[] { ":" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        // Strip the command or colon
        string action = input.StartsWith(":")
            ? input.Substring(1).Trim()
            : input.Substring(input.IndexOf(' ') + 1).Trim();

        await session.SendLineAsync($"{player.Username} {action}");

        // Later: broadcast to other players in room
    }
}
