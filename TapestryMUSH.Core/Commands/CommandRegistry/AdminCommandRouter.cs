using TapestryMUSH.Core.Commands.Admin;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public static class AdminCommandRouter
{
    public static IEnumerable<ICommand> Register()
    {
        return new ICommand[]
        {
            new SetCommand()
        };
    }
}