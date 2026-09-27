using Cli.Engine;

using Infra.Display;
using Cli.Presenter;
namespace Cli.Text
{
    /// <summary>
    /// Moves the current position to the parent container.
    /// </summary>
    public sealed class UpCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;
        private readonly TextPresenter _presenter;

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
        /// Initializes an up command bound to the given navigator and presenter.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="presenter">The text presenter.</param>
        public UpCommand(TextNavigator navigator, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _presenter = presenter;
        }

        /// <summary>
        /// Moves up and prints the new position.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line (validated, takes no arguments).</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage);
            _navigator.Up();
            _presenter.ShowMoved(_navigator.Pwd());
        }
    }
}
