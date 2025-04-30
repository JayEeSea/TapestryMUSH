using System.Collections.Concurrent;
using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public class CommandRouter
{
    private readonly Dictionary<string, ICommand> _commandMap = new();

    public CommandRouter(IEnumerable<ICommand> commands)
    {
        foreach (var cmd in commands)
        {
            _commandMap[cmd.Name] = cmd;

            foreach (var alias in cmd.Aliases)
            {
                _commandMap[alias] = cmd;
            }
        }
    }

    public async Task RouteAsync(ClientSession session, string input)
    {
        input = input.Trim();

        if (string.IsNullOrWhiteSpace(input))
            return;

        // Handle MUSH-style command shorthands
        if (input.StartsWith("\""))
        {
            input = "say " + input.Substring(1).Trim();
        }
        else if (input.StartsWith(":"))
        {
            input = "pose " + input.Substring(1).Trim();
        }

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var commandName = parts[0].ToLowerInvariant();

        if (_commandMap.TryGetValue(commandName, out var command))
        {
            await command.ExecuteAsync(session, input);
            return;
        }

        // Fallback: Check if it's a valid exit in the current room
        var player = await session.AccountService.GetPlayerByClientAsync(session);
        if (player?.CurrentRoom != null)
        {
            var exits = player.CurrentRoom.Exits;
            if (exits.TryGetValue(commandName, out int destRoomId))
            {
                var db = session.AccountService.DbContext;
                var dest = await db.Rooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == destRoomId);

                if (dest != null)
                {
                    player.CurrentRoomId = dest.Id;
                    await db.SaveChangesAsync();

                    await session.SendLineAsync($"You go {commandName}.");
                    await session.SendLineAsync(dest.Name);
                    await session.SendLineAsync(dest.Description);
                    return;
                }
            }
        }

        await session.SendLineAsync("Huh? (Type \"help\" for help.)");
    }

    public void Register(ICommand command)
    {
        _commandMap[command.Name] = command;

        foreach (var alias in command.Aliases)
        {
            _commandMap[alias] = command;
        }
    }
}
