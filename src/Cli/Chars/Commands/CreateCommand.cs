using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
using Cli.Presenter;
namespace Cli.Chars
{
    /// <summary>
    /// Creates characters, items and abilities through a question dialog.
    /// </summary>
    public sealed class CreateCommand : IShellCommand
    {
        private readonly CharsWorld _world;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "create";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Create a character, item or ability via dialog.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "create <char|item|ability>";

        /// <summary>
        /// Initializes a create command bound to the given world.
        /// </summary>
        /// <param name="world">The characters world.</param>
        public CreateCommand(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            _world = world;
        }

        /// <summary>
        /// Runs the creation dialog for the requested category.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string kind = command.RequireArg(0, "a category: create <char|item|ability>.");
            switch (kind.ToLowerInvariant())
            {
                case "char":
                    CreateCharacter(console);
                    break;
                case "item":
                    CreateItem(console);
                    break;
                case "ability":
                    CreateAbility(console);
                    break;
                default:
                    throw new CommandException($"Unknown category '{kind}'. Use char, item or ability.");
            }
        }

        /// <summary>
        /// Runs the character creation dialog.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        private void CreateCharacter(IDisplay console)
        {
            string name = Prompter.ReadRequired(console, "Name");
            int maxHealth = Prompter.ReadInt(console, "MaxHealth", 1, 10000);
            int armor = Prompter.ReadInt(console, "BaseArmor", 0, 1000);
            int attack = Prompter.ReadInt(console, "BaseAttack", 0, 10000);
            string? id = Prompter.ReadOptional(console, "Id (empty for auto)");

            var character = new Character(name, maxHealth, armor, attack);
            string assigned = _world.Characters.Add(character, id);
            console.WriteLine($"Created character '{name}' [{assigned}] ({maxHealth} HP, {armor} ARM, {attack} ATK).");
        }

        /// <summary>
        /// Runs the item creation dialog.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        private void CreateItem(IDisplay console)
        {
            string name = Prompter.ReadRequired(console, "Name");
            int attack = Prompter.ReadInt(console, "AttackBonus", 0, 10000);
            int armor = Prompter.ReadInt(console, "ArmorBonus", 0, 10000);
            string? id = Prompter.ReadOptional(console, "Id (empty for auto)");

            var item = new Equipment(name, attack, armor);
            string assigned = _world.Items.Add(item, id);
            console.WriteLine($"Created item '{name}' [{assigned}] (+{attack} ATK, +{armor} ARM).");
        }

        /// <summary>
        /// Runs the ability creation dialog.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        private void CreateAbility(IDisplay console)
        {
            string name = Prompter.ReadRequired(console, "Name");
            int multiplier = Prompter.ReadInt(console, "DamageMultiplier", 1, 100);
            string? id = Prompter.ReadOptional(console, "Id (empty for auto, must be free)");

            var ability = new Ability(name, multiplier);
            string assigned = _world.Abilities.Add(ability, id);
            console.WriteLine($"Created ability '{name}' [{assigned}] (x{multiplier}).");
        }
    }
}
