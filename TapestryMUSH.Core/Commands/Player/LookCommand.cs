using TapestryMUSH.Core.Session;
using static TapestryMUSH.Data.Models.Player;

namespace TapestryMUSH.Core.Commands.Player;

public class LookCommand : ICommand
{
    public string Name => "look";
    public IEnumerable<string> Aliases => new[] { "l" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);

        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        if (player.CurrentRoom == null)
        {
            await session.SendLineAsync("You are nowhere.");
            return;
        }

        var showDbref = player.Flags.Contains("WIZARD") || player.Flags.Contains("ROYALTY");
        var roomDisplay = showDbref
            ? $"{player.CurrentRoom.Name} [#{player.CurrentRoom.Id}]"
            : player.CurrentRoom.Name;

        await session.SendLineAsync(roomDisplay);
        await session.SendLineAsync(player.CurrentRoom.Description);

        if (player.CurrentRoom.Exits != null && player.CurrentRoom.Exits.Count > 0)
        {
            var exitList = string.Join(", ", player.CurrentRoom.Exits.Keys);
            await session.SendLineAsync("Obvious exits: " + exitList);
        }
        else
        {
            await session.SendLineAsync("Obvious exits: None.");
        }
    }
}
