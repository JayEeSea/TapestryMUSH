using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Commands.Player;

public class NameCommand : ICommand
{
    public string Name => "@name";
    public IEnumerable<string> Aliases => Array.Empty<string>();

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        var parts = input.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !parts[0].Trim().Equals("@name here", StringComparison.OrdinalIgnoreCase))
        {
            await session.SendLineAsync("Usage: @name here=<new name>");
            return;
        }

        if (player.CurrentRoom == null)
        {
            await session.SendLineAsync("You are nowhere.");
            return;
        }

        // Optional: Check if the player is a WIZARD or ROYALTY
        var isAdmin = player.Flags.Contains("WIZARD") || player.Flags.Contains("ROYALTY");

        // In the future: check ownership here if not admin

        var newName = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            await session.SendLineAsync("You must provide a valid new name.");
            return;
        }

        var db = session.AccountService.DbContext;
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == player.CurrentRoomId);

        if (room == null)
        {
            await session.SendLineAsync("Room not found.");
            return;
        }

        room.Name = newName;
        await db.SaveChangesAsync();

        await session.SendLineAsync($"Room renamed to: {newName}");
    }
}