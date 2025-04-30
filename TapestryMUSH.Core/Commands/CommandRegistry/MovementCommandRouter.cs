using TapestryMUSH.Core.Commands.Movement;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public static class MovementCommandRouter
{
    public static IEnumerable<ICommand> Register()
    {
        return new ICommand[]
        {
            new GoCommand(),
            new TeleportCommand()
        };
    }
}