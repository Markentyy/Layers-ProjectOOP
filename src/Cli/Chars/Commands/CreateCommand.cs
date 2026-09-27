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
        private readonly CharsPresenter _presenter;

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
        /// Initializes a create command bound to the given world and presenter.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="presenter">The characters presenter.</param>
        public CreateCommand(CharsWorld world, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _presenter = presenter;
        }

        /// <summary>
        /// Runs the creation dialog for the requested category.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string kind = command.RequireArg(0, "a category: create <char|item|ability>.");
            switch (kind.ToLowerInvariant())
            {
                case "char":
                    CreateCharacter(display);
                    break;
                case "item":
                    CreateItem(display);
                    break;
                case "ability":
                    CreateAbility(display);
                    break;
                default:
                    throw new CommandException($"Unknown category '{kind}'. Use char, item or ability.");
            }
        }

        /// <summary>
        /// Runs the character creation dialog.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        private void CreateCharacter(IDisplay display)
        {
            string name = Prompter.ReadRequired(display, "Name");
            int maxHealth = Prompter.ReadInt(display, "MaxHealth", 1, 10000);
            int armor = Prompter.ReadInt(display, "BaseArmor", 0, 1000);
            int attack = Prompter.ReadInt(display, "BaseAttack", 0, 10000);
            string? id = Prompter.ReadOptional(display, "Id (empty for auto)");

            var character = new Character(name, maxHealth, armor, attack);
            string assigned = _world.AddCharacter(character, id);
            _presenter.ShowCharacterCreated(assigned, name, maxHealth, armor, attack);
        }

        /// <summary>
        /// Runs the item creation dialog.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        private void CreateItem(IDisplay display)
        {
            string name = Prompter.ReadRequired(display, "Name");
            int attack = Prompter.ReadInt(display, "AttackBonus", 0, 10000);
            int armor = Prompter.ReadInt(display, "ArmorBonus", 0, 10000);
            string? id = Prompter.ReadOptional(display, "Id (empty for auto)");

            var item = new Equipment(name, attack, armor);
            string assigned = _world.AddItem(item, id);
            _presenter.ShowItemCreated(assigned, name, attack, armor);
        }

        /// <summary>
        /// Runs the ability creation dialog.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        private void CreateAbility(IDisplay display)
        {
            string name = Prompter.ReadRequired(display, "Name");
            int multiplier = Prompter.ReadInt(display, "DamageMultiplier", 1, 100);
            string? id = Prompter.ReadOptional(display, "Id (empty for auto, must be free)");

            var ability = new Ability(name, multiplier);
            string assigned = _world.AddAbility(ability, id);
            _presenter.ShowAbilityCreated(assigned, name, multiplier);
        }
    }
}
