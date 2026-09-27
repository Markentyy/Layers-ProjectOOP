using Cli.Engine;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

namespace Cli.Chars
{
    /// <summary>
    /// Restores the characters world state from a JSON file.
    /// </summary>
    public sealed class LoadCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly ICharsStore _store;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "load";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Load the characters world from a file.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "load <file>";

        /// <summary>
        /// Initializes a load command.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="store">The characters store.</param>
        /// <param name="presenter">The characters presenter.</param>
        public LoadCommand(CharsWorld world, ICharsStore store, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _store = store;
            _presenter = presenter;
        }

        /// <summary>
        /// Reads the file and refills the world.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string path = command.Args[0];
            CharsSnapshot snapshot;
            try
            {
                snapshot = _store.Load(path);
            }
            catch (StoreException ex)
            {
                throw new CommandException(ex.Message);
            }
            CharsWorldMapper.LoadInto(_world, snapshot);
            _presenter.ShowLoaded(path, snapshot.Characters.Count, snapshot.Items.Count, snapshot.Abilities.Count);
        }
    }
}
