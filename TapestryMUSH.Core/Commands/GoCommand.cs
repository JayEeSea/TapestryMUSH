using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Commands;

public class GoCommand : ICommand
{
    public string Name => "go";
    public IEnumerable<string> Aliases => new[] { "goto", "move" };

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player?.CurrentRoom == null)
        {
            await session.SendLineAsync("You are nowhere.");
            return;
        }

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await session.SendLineAsync("Usage: go <direction>");
            return;
        }

        var direction = parts[1].ToLowerInvariant();
        var db = session.AccountService.DbContext;

        var currentRoom = await db.Rooms
            .Include(r => r.NorthRoom)
            .Include(r => r.SouthRoom)
            .Include(r => r.EastRoom)
            .Include(r => r.WestRoom)
            .FirstOrDefaultAsync(r => r.Id == player.CurrentRoomId);

        Room? destination = direction switch
        {
            "north" => currentRoom?.NorthRoom,
            "south" => currentRoom?.SouthRoom,
            "east" => currentRoom?.EastRoom,
            "west" => currentRoom?.WestRoom,
            _ => null
        };

        if (destination == null)
        {
            await session.SendLineAsync("You can't go that way.");
            return;
        }

        player.CurrentRoomId = destination.Id;
        await db.SaveChangesAsync();

        await session.SendLineAsync($"You go {direction}.");
        await session.SendLineAsync(destination.Name);
        await session.SendLineAsync(destination.Description);
    }
}
