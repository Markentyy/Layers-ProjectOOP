using Cli.Engine;
using Cli.Text;
using Core.GameSystem;
using Infra.Display;
using Cli.Presenter;

using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// Prints the full state of a character as a structured text sheet.
    /// </summary>
    public sealed class ShowCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "show";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Show a character state as a structured text sheet.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "show <id|name>";

        /// <summary>
        /// Initializes a show command.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="presenter">The characters presenter.</param>
        public ShowCommand(CharsWorld world, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _presenter = presenter;
        }

        /// <summary>
        /// Builds and prints the character sheet.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            KeyValuePair<string, Character> entry = _world.ResolveCharacter(command.Args[0]);
            _world.Lore.TryGetValue(entry.Key, out Infra.WebApi.GenshinCharacterDto? lore);

            List<KeyValuePair<string, Equipment>> items = new();
            foreach (Equipment item in entry.Value.Inventory.Items)
            {
                _world.Items.TryGetId(item, out string? itemId);
                items.Add(new KeyValuePair<string, Equipment>(itemId ?? "?", item));
            }
            List<KeyValuePair<string, Ability>> abilities = new();
            foreach (string abilityId in _world.BookOf(entry.Key))
            {
                abilities.Add(new KeyValuePair<string, Ability>(abilityId, _world.Abilities.GetById(abilityId)));
            }

            Core.TextSystem.TextDocument sheet = CharacterSheet.Build(entry.Key, entry.Value, lore, items, abilities);
            _presenter.ShowSheet(sheet.RenderDocument());
        }
    }
}
