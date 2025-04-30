using TapestryMUSH.Core.Session;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Data.Models;
using TapestryMUSH.Shared;

namespace TapestryMUSH.Core.Commands.Admin;

public class SetCommand : ICommand
{
    public string Name => "@set";
    public IEnumerable<string> Aliases => Array.Empty<string>();

    public async Task ExecuteAsync(ClientSession session, string input)
    {
        var invoker = await session.AccountService.GetPlayerByClientAsync(session);
        if (invoker == null)
        {
            await session.SendLineAsync("You are not connected.");
            return;
        }

        var parts = input.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            await session.SendLineAsync("Usage: @set <target> = <flag>");
            return;
        }

        var target = parts[0].Replace("@set", "", StringComparison.OrdinalIgnoreCase).Trim();
        var isRemoval = parts[1].StartsWith("!");
        var flagInput = parts[1].TrimStart('!').ToUpperInvariant();

        var db = session.AccountService.DbContext;
        TapestryMUSH.Data.Models.Player? targetPlayer = null;

        if (target.Equals("me", StringComparison.OrdinalIgnoreCase))
        {
            targetPlayer = invoker;
        }
        else if (target.StartsWith("#") && int.TryParse(target.Substring(1), out int id))
        {
            targetPlayer = await db.Players.FirstOrDefaultAsync(p => p.Id == id);
        }
        else
        {
            targetPlayer = await db.Players.FirstOrDefaultAsync(p => p.Username.ToLower() == target.ToLower());
        }

        if (targetPlayer == null)
        {
            await session.SendLineAsync("Target player not found.");
            return;
        }

        // Match against currently known flags stored in TapestryMUSH.Shared/PlayerFlagRegistry.cs
        var knownFlags = PlayerFlagRegistry.ValidFlags;

        var possibleMatches = knownFlags
            .Where(name => name.StartsWith(flagInput, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (possibleMatches.Count == 0)
        {
            await session.SendLineAsync($"Unknown flag: {flagInput}. Use one of: {string.Join(", ", knownFlags)}");
            return;
        }
        else if (possibleMatches.Count > 1)
        {
            await session.SendLineAsync($"Ambiguous flag: {flagInput}. Matches: {string.Join(", ", possibleMatches)}");
            return;
        }

        var matchedFlag = possibleMatches[0];

        // Only allow setting WIZARD or ROYALTY if the invoker is a wizard.
        if ((matchedFlag == "WIZARD" || matchedFlag == "ROYALTY") &&
            !invoker.Flags.Contains("WIZARD"))
        {
            await session.SendLineAsync("Permission denied.");
            return;
        }

        // Apply the flag
        var flagToSet = possibleMatches[0].ToUpperInvariant();
        var flags = targetPlayer.Flags;

        if (isRemoval)
        {
            if (!flags.Contains(flagToSet))
            {
                await session.SendLineAsync($"{targetPlayer.Username} does not have the {flagToSet} flag.");
                return;
            }

            // Special case: don't let non-wizards remove WIZARD or ROYALTY
            if ((flagToSet == "WIZARD" || flagToSet == "ROYALTY") &&
                !invoker.Flags.Contains("WIZARD"))
            {
                await session.SendLineAsync("Permission denied.");
                return;
            }

            flags.Remove(flagToSet);
            await session.SendLineAsync($"Flag {flagToSet} removed from {targetPlayer.Username} [#{targetPlayer.Id}].");
        }
        else
        {
            if (flags.Contains(flagToSet))
            {
                await session.SendLineAsync($"{targetPlayer.Username} already has the {flagToSet} flag.");
                return;
            }

            // Only allow setting WIZARD or ROYALTY if the invoker is a wizard
            if ((flagToSet == "WIZARD" || flagToSet == "ROYALTY") &&
                !invoker.Flags.Contains("WIZARD"))
            {
                await session.SendLineAsync("Permission denied.");
                return;
            }

            flags.Add(flagToSet);
            await session.SendLineAsync($"Flag {flagToSet} set on {targetPlayer.Username} [#{targetPlayer.Id}].");
        }

        targetPlayer.Flags = flags;

        await db.SaveChangesAsync();
    }
}