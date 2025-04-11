using TapestryMUSH.Core;
using TapestryMUSH.Core.Session;

namespace TapestryMUSH.Core.Commands;

public interface ICommand
{
    string Name { get; }                     // e.g. "look"
    IEnumerable<string> Aliases { get; }     // e.g. ["l"]
    Task ExecuteAsync(ClientSession session, string input);
}
