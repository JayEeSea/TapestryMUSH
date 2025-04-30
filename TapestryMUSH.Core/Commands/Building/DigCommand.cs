using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Core.Commands.Building;

public class DigCommand : ICommand
{
    public string Name => "@dig";
    public IEnumerable<string> Aliases => Array.Empty<string>();

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var parts = input.Split('=', 2, StringSplitOptions.TrimEntries);

        if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
        {
            await session.SendLineAsync("Usage: @dig <room name> = <room description>");
            return;
        }

        var name = parts[0].Replace("@dig", "", StringComparison.OrdinalIgnoreCase).Trim();
        var description = parts.Length > 1 ? parts[1] : "An empty room.";

        if (string.IsNullOrWhiteSpace(name))
        {
            await session.SendLineAsync("Room name cannot be empty.");
            return;
        }

        var db = session.AccountService.DbContext;

        var room = new Room
        {
            Name = name,
            Description = description,
            ExitsJson = "{}"
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        await session.SendLineAsync($"Room created: {room.Name} (ID #{room.Id})");
    }
}
