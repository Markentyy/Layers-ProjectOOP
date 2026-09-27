using Cli.Engine;

using Infra.Display;
namespace Cli.Text
{
    /// <summary>
    /// Prints the current path in the document.
    /// </summary>
    public sealed class PwdCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "pwd";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Show the current path in the document.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "pwd";

        /// <summary>
        /// Initializes a pwd command bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        public PwdCommand(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            _navigator = navigator;
        }

        /// <summary>
        /// Prints the current path.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line (validated, takes no arguments).</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage);
            console.WriteLine(_navigator.Pwd());
        }
    }
}
