using Cli.Engine;
using Core.TextSystem;

using Infra.Display;
using Cli.Presenter;
namespace Cli.Text
{
    /// <summary>
    /// Removes a named child of the current container, or the current
    /// container itself when no name is given. Always asks for confirmation.
    /// </summary>
    public sealed class RmCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;
        private readonly TextPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "rm";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Remove a child element or the current container.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "rm [<elem_name>]";

        /// <summary>
        /// Initializes a remove command bound to the given navigator and presenter.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="presenter">The text presenter.</param>
        public RmCommand(TextNavigator navigator, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _presenter = presenter;
        }

        /// <summary>
        /// Confirms and performs the removal.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(0, 1, Usage);
            command.ExpectOptions(Usage);
            if (command.Args.Count == 0)
            {
                RemoveCurrent(display);
                return;
            }

            string name = command.Args[0];
            IReadOnlyList<TextElement> found = _navigator.FindInCurrent(name);
            if (found.Count == 0)
                throw new CommandException($"No element '{name}' here.");
            if (found.Count > 1)
                throw new CommandException($"Ambiguous name '{name}': {found.Count} matches.");

            TextElement element = found[0];
            if (!Prompter.ReadYesNo(display, $"Delete '{element.DisplayName}' [{element.Id}]"))
                return;
            _navigator.RemoveFromCurrent(element);
            _presenter.ShowRemoved(element.DisplayName);
        }

        /// <summary>
        /// Confirms and removes the current container, then moves to its parent.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        private void RemoveCurrent(IDisplay display)
        {
            if (_navigator.IsRoot)
                throw new CommandException("Nothing selected: the current position is the document root.");
            string title = _navigator.CurrentTitle;
            if (!Prompter.ReadYesNo(display, $"Delete current section '{title}' and go up"))
                return;
            Section removed = _navigator.RemoveCurrent();
            _presenter.ShowRemovedCurrent(removed.DisplayName, _navigator.Pwd());
        }
    }
}
