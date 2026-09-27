using Cli.Engine;

using Infra.Display;
namespace Cli.Text
{
    /// <summary>
    /// Prints the current element or the whole document, optionally with ids.
    /// </summary>
    public sealed class PrintCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "print";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Print the current element or the whole document.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "print [--whole] [--id]";

        /// <summary>
        /// Initializes a print command bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        public PrintCommand(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            _navigator = navigator;
        }

        /// <summary>
        /// Renders the requested scope to the console.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage, "whole", "id");
            bool showIds = command.HasOption("id");
            string text = command.HasOption("whole")
                ? _navigator.RenderWhole(showIds)
                : _navigator.RenderCurrent(showIds);
            console.Write(text);
        }
    }
}
