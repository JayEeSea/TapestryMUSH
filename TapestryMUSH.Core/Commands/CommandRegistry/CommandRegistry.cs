using System.Collections.Generic;

namespace TapestryMUSH.Core.Commands.CommandRegistry;
public static class CommandRegistry
{
    public static CommandRouter BuildRouter()
    {
        var allCommands = new List<ICommand>();

        allCommands.AddRange(AdminCommandRouter.Register());
        allCommands.AddRange(BuildingCommandRouter.Register());
        allCommands.AddRange(CommunicationCommandRouter.Register());
        allCommands.AddRange(MovementCommandRouter.Register());
        allCommands.AddRange(PlayerCommandRouter.Register());

        return new CommandRouter(allCommands);
    }
}