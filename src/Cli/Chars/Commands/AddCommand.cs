using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
namespace Cli.Chars
{
    /// <summary>
    /// Attaches an item or an ability to a character.
    /// </summary>
    public sealed class AddCommand : IShellCommand
    {
        private readonly CharsWorld _world;

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
        /// Initializes an add command bound to the given world.
        /// </summary>
        /// <param name="world">The characters world.</param>
        public AddCommand(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            _world = world;
        }

        /// <summary>
        /// Equips an item or teaches an ability to the selected character.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
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
                    console.WriteLine($"{character.Value.Name} already has {item.Value.Name} equipped.");
                    return;
                }
                character.Value.Equip(item.Value);
                console.WriteLine($"{character.Value.Name} equipped {item.Value.Name} " +
                    $"(+{item.Value.AttackBonus} ATK, +{item.Value.ArmorBonus} ARM).");
                return;
            }

            if (_world.Abilities.ContainsId(target) || _world.Abilities.IdsOfName(target).Count > 0)
            {
                KeyValuePair<string, Ability> ability = _world.ResolveAbility(target);
                if (!_world.Learn(character.Key, ability.Key))
                {
                    console.WriteLine($"{character.Value.Name} already knows {ability.Value.Name}.");
                    return;
                }
                console.WriteLine($"{character.Value.Name} learned {ability.Value.Name} [{ability.Key}].");
                return;
            }

            throw new CommandException($"Unknown item or ability '{target}'.");
        }
    }
}
