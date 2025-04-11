using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;
using static TapestryMUSH.Data.Models.Player;

namespace TapestryMUSH.Core.Commands;

public class TeleportCommand : ICommand
{
    public string Name => "@tel";
    public IEnumerable<string> Aliases => new[] { "@teleport" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        // Only allow Wizards or Royalty allowed to teleport
        if (!player.Flags.Contains("WIZARD") || player.Flags.Contains("ROYALTY"))
        {
            await session.SendLineAsync("Permission denied.");
            return;
        }

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await session.SendLineAsync("Usage: @tel #<room id>");
            return;
        }

        var idPart = parts[1];
        if (idPart.StartsWith("#"))
            idPart = idPart.Substring(1);

        if (!int.TryParse(idPart, out int destRoomId))
        {
            await session.SendLineAsync("Invalid room ID.");
            return;
        }

        var db = session.AccountService.DbContext;
        var destination = await db.Rooms.FirstOrDefaultAsync(r => r.Id == destRoomId);

        if (destination == null)
        {
            await session.SendLineAsync($"Room #{destRoomId} does not exist.");
            return;
        }

        player.CurrentRoomId = destination.Id;
        await db.SaveChangesAsync();

        // Reload to refresh CurrentRoom
        player = await db.Players
            .Include(p => p.CurrentRoom)
            .FirstOrDefaultAsync(p => p.Id == player.Id);
        
        session.CurrentPlayer = player;

        // Show the teleport success message
        await session.SendLineAsync($"You teleport to {destination.Name} [#{destination.Id}].");

        // Immediately do a look command as if the player typed it
        await session.CommandRouter.RouteAsync(session, "look");

        await session.SendLineAsync($"You teleport to {destination.Name} [#{destination.Id}].");
        await session.CommandRouter.RouteAsync(session, "look");
    }
}
