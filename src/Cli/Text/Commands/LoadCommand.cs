using Cli.Engine;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

namespace Cli.Text
{
    /// <summary>
    /// Restores the text document from a JSON file and returns to the root.
    /// </summary>
    public sealed class LoadCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;
        private readonly ITextStore _store;
        private readonly TextPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "load";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Load the document from a file.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "load <file>";

        /// <summary>
        /// Initializes a load command.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="store">The text store.</param>
        /// <param name="presenter">The text presenter.</param>
        public LoadCommand(TextNavigator navigator, ITextStore store, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _store = store;
            _presenter = presenter;
        }

        /// <summary>
        /// Reads the file and replaces the explored document.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string path = command.Args[0];
            TextSnapshot snapshot;
            try
            {
                snapshot = _store.Load(path);
            }
            catch (StoreException ex)
            {
                throw new CommandException(ex.Message);
            }
            _navigator.ReplaceDocument(TextMapper.ToDocument(snapshot));
            _presenter.ShowLoaded(path);
        }
    }
}
