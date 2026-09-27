using Cli.Engine;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

namespace Cli.Text
{
    /// <summary>
    /// Saves the text document to a JSON file.
    /// </summary>
    public sealed class SaveCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;
        private readonly ITextStore _store;
        private readonly TextPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "save";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Save the document to a file.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "save <file>";

        /// <summary>
        /// Initializes a save command.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="store">The text store.</param>
        /// <param name="presenter">The text presenter.</param>
        public SaveCommand(TextNavigator navigator, ITextStore store, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _store = store;
            _presenter = presenter;
        }

        /// <summary>
        /// Captures the document and writes it to the file.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string path = command.Args[0];
            try
            {
                _store.Save(TextMapper.ToSnapshot(_navigator.Document), path);
            }
            catch (StoreException ex)
            {
                throw new CommandException(ex.Message);
            }
            _presenter.ShowSaved(path);
        }
    }
}
