using Cli.Engine;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

namespace Cli.Chars
{
    /// <summary>
    /// Saves the characters world state to a JSON file.
    /// </summary>
    public sealed class SaveCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly ICharsStore _store;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "save";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Save the characters world to a file.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "save <file>";

        /// <summary>
        /// Initializes a save command.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="store">The characters store.</param>
        /// <param name="presenter">The characters presenter.</param>
        public SaveCommand(CharsWorld world, ICharsStore store, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _store = store;
            _presenter = presenter;
        }

        /// <summary>
        /// Captures the world and writes it to the file.
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
                _store.Save(CharsWorldMapper.ToSnapshot(_world), path);
            }
            catch (StoreException ex)
            {
                throw new CommandException(ex.Message);
            }
            _presenter.ShowSaved(path);
        }
    }
}
