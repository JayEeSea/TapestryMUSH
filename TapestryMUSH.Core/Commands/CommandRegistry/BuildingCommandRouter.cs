using TapestryMUSH.Core.Commands.Building;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public static class BuildingCommandRouter
{
    public static IEnumerable<ICommand> Register()
    {
        return new ICommand[]
        {
            new DigCommand(),
            new OpenCommand()
        };
    }
}