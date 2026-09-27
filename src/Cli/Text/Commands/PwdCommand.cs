using Cli.Engine;

using Infra.Display;
using Cli.Presenter;
namespace Cli.Text
{
    /// <summary>
    /// Prints the current path in the document.
    /// </summary>
    public sealed class PwdCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;
        private readonly TextPresenter _presenter;

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
        /// Initializes a pwd command bound to the given navigator and presenter.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="presenter">The text presenter.</param>
        public PwdCommand(TextNavigator navigator, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _presenter = presenter;
        }

        /// <summary>
        /// Prints the current path.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line (validated, takes no arguments).</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage);
            _presenter.ShowPath(_navigator.Pwd());
        }
    }
}
