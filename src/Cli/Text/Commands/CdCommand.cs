using Cli.Engine;

using Infra.Display;
namespace Cli.Text
{
    /// <summary>
    /// Moves the current position to the given path or container id.
    /// </summary>
    public sealed class CdCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "cd";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Go to a path or to a container by id.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "cd <path> | cd --id <container_id>";

        /// <summary>
        /// Initializes a cd command bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        public CdCommand(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            _navigator = navigator;
        }

        /// <summary>
        /// Navigates to the requested path or container and prints the new position.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(0, 1, Usage);
            command.ExpectOptions(Usage, "id");
            string? id = command.GetOption("id");
            if (id is not null)
            {
                if (command.Args.Count > 0)
                    throw new CommandException("Use either a path or --id, not both.");
                _navigator.CdById(id);
            }
            else
            {
                _navigator.Cd(command.RequireArg(0, "a path: cd <path>."));
            }
            console.WriteLine($"Current: {_navigator.Pwd()}.");
        }
    }
}
