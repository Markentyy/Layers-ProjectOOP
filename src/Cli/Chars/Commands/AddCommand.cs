using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
using Cli.Presenter;
namespace Cli.Chars
{
    /// <summary>
    /// Attaches an item or an ability to a character.
    /// </summary>
    public sealed class AddCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "add";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Give an item or ability to a character.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "add --char_id <id|name> --id <id|name>";

        /// <summary>
        /// Initializes an add command bound to the given world and presenter.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="presenter">The characters presenter.</param>
        public AddCommand(CharsWorld world, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _presenter = presenter;
        }

        /// <summary>
        /// Equips an item or teaches an ability to the selected character.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(0, 0, Usage);
            command.ExpectOptions(Usage, "char_id", "id");
            string charRef = command.RequireOption("char_id");
            string target = command.RequireOption("id");
            KeyValuePair<string, Character> character =
                _world.ResolveCharacter(charRef);

            if (_world.Items.ContainsId(target) || _world.Items.IdsOfName(target).Count > 0)
            {
                KeyValuePair<string, Equipment> item = _world.ResolveItem(target);
                if (character.Value.Inventory.Items.Contains(item.Value))
                {
                    _presenter.ShowAlreadyEquipped(character.Value.Name, item.Value.Name);
                    return;
                }
                character.Value.Equip(item.Value);
                _presenter.ShowEquipped(character.Value.Name, item.Value.Name, item.Value.AttackBonus, item.Value.ArmorBonus);
                return;
            }

            if (_world.Abilities.ContainsId(target) || _world.Abilities.IdsOfName(target).Count > 0)
            {
                KeyValuePair<string, Ability> ability = _world.ResolveAbility(target);
                if (!_world.Learn(character.Key, ability.Key))
                {
                    _presenter.ShowAlreadyKnows(character.Value.Name, ability.Value.Name);
                    return;
                }
                _presenter.ShowLearned(character.Value.Name, ability.Value.Name, ability.Key);
                return;
            }

            throw new CommandException($"Unknown item or ability '{target}'.");
        }
    }
}
