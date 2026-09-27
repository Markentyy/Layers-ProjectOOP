using Cli.Engine;

using Infra.Display;
namespace Cli.Text
{
    /// <summary>
    /// Moves the current position to the parent container.
    /// </summary>
    public sealed class UpCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "up";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Go to the parent container.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "up";

        /// <summary>
        /// Initializes an up command bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        public UpCommand(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            _navigator = navigator;
        }

        /// <summary>
        /// Moves up and prints the new position.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line (validated, takes no arguments).</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage);
            _navigator.Up();
            console.WriteLine($"Current: {_navigator.Pwd()}.");
        }
    }
}
