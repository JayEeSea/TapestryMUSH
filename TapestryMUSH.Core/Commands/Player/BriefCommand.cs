using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;

namespace TapestryMUSH.Core.Commands.Player;

public class BriefCommand : ICommand
{
    public string Name => "brief";
    public IEnumerable<string> Aliases => new[] { "br" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);

        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        string nameLine = $"{player.Username}(#{player.Id})";
        string flagsLine = $"Flags: {string.Join(" ", player.Flags)}";
        var localCreated = player.DateCreated.ToLocalTime();
        string createdLine = $"Created: {localCreated:ddd MMM dd HH:mm:ss yyyy}";

        string locationLine = player.CurrentRoom != null
            ? $"Location: {player.CurrentRoom.Name}(#{player.CurrentRoom.Id})"
            : "Location: *nowhere*";

        await session.SendLineAsync(nameLine);
        await session.SendLineAsync(flagsLine);
        await session.SendLineAsync(createdLine);
        await session.SendLineAsync(locationLine);
    }
}
