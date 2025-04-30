using TapestryMUSH.Core.Commands.Admin;
using TapestryMUSH.Core.Commands.Player;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public static class PlayerCommandRouter
{
    public static IEnumerable<ICommand> Register()
    {
        return new ICommand[]
        {
            new BriefCommand(),
            new LookCommand(),
            new NameCommand()
        };
    }
}