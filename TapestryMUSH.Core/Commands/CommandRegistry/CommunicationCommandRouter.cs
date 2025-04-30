using TapestryMUSH.Core.Commands.Communication;

namespace TapestryMUSH.Core.Commands.CommandRegistry;

public static class CommunicationCommandRouter
{
    public static IEnumerable<ICommand> Register()
    {
        return new ICommand[]
        {
            new SayCommand(),
            new PoseCommand()
        };
    }
}
