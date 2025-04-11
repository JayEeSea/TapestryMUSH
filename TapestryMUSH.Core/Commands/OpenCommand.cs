using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;

namespace TapestryMUSH.Core.Commands;

public class OpenCommand : ICommand
{
    public string Name => "@open";
    public IEnumerable<string> Aliases => Array.Empty<string>();

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var player = await session.AccountService.GetPlayerByClientAsync(session);

        if (player?.CurrentRoom == null)
        {
            await session.SendLineAsync("You are not in a room.");
            return;
        }

        var parts = input.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            await session.SendLineAsync("Usage: @open <exit name> = <room id>");
            return;
        }

        var exitName = parts[0].Replace("@open", "", StringComparison.OrdinalIgnoreCase).Trim().ToLowerInvariant();
        var destPart = parts[1].Trim();

        if (destPart.StartsWith("#"))
            destPart = destPart.Substring(1); // Remove the #

        if (!int.TryParse(destPart, out var destId))
        {
            await session.SendLineAsync("Invalid room ID. Use @open <exit> = #<roomID>");
            return;
        }

        var db = session.AccountService.DbContext;
        var currentRoom = await db.Rooms.FirstOrDefaultAsync(r => r.Id == player.CurrentRoomId);

        if (currentRoom == null)
        {
            await session.SendLineAsync("Current room not found.");
            return;
        }

        var destinationRoom = await db.Rooms.FirstOrDefaultAsync(r => r.Id == destId);
        if (destinationRoom == null)
        {
            await session.SendLineAsync($"Room ID {destId} does not exist.");
            return;
        }

        var exits = currentRoom.Exits;
        if (exits.ContainsKey(exitName))
        {
            await session.SendLineAsync($"An exit named '{exitName}' already exists.");
            return;
        }

        exits[exitName] = destId;
        currentRoom.Exits = exits;
        await db.SaveChangesAsync();

        await session.SendLineAsync($"Exit '{exitName}' created leading to {destinationRoom.Name} (ID {destId}).");
    }
}
