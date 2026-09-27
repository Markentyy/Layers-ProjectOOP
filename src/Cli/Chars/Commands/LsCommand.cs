using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
using Cli.Presenter;
namespace Cli.Chars
{
    /// <summary>
    /// Lists stored objects in short form, or one object in full detail.
    /// </summary>
    public sealed class LsCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "ls";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "List objects, or show one object in detail.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "ls <char|item|ability> [--id <id|name>]";

        /// <summary>
        /// Initializes a list command bound to the given world and presenter.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="presenter">The characters presenter.</param>
        public LsCommand(CharsWorld world, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _presenter = presenter;
        }

        /// <summary>
        /// Prints the short category listing or the full object state.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage, "id");
            string category = command.RequireArg(0, "a category: ls <char|item|ability>.");
            string? selected = command.GetOption("id");

            switch (category.ToLowerInvariant())
            {
                case "char":
                    if (selected is null)
                        ListCharacters();
                    else
                        ShowCharacter(_world.ResolveCharacter(selected));
                    break;
                case "item":
                    if (selected is null)
                        ListItems();
                    else
                        ShowItem(_world.ResolveItem(selected));
                    break;
                case "ability":
                    if (selected is null)
                        ListAbilities();
                    else
                        ShowAbility(_world.ResolveAbility(selected));
                    break;
                default:
                    throw new CommandException($"Unknown category '{category}'. Use char, item or ability.");
            }
        }

        /// <summary>
        /// Prints one line per character.
        /// </summary>
        private void ListCharacters()
        {
            if (_world.Characters.Count == 0)
            {
                _presenter.ShowEmptyList("characters", "create char");
                return;
            }
            foreach (KeyValuePair<string, Character> entry in _world.Characters.All)
            {
                Character c = entry.Value;
                _presenter.ShowCharacterShort(entry.Key, c.Name, c.Health, c.MaxHealth, c.TotalAttack, c.TotalArmor, c.IsDefeated);
            }
        }

        /// <summary>
        /// Prints one line per item with its users.
        /// </summary>
        private void ListItems()
        {
            if (_world.Items.Count == 0)
            {
                _presenter.ShowEmptyList("items", "create item");
                return;
            }
            foreach (KeyValuePair<string, Equipment> entry in _world.Items.All)
            {
                string users = string.Join(", ", _world.UsersOfItem(entry.Value).Select(u => u.Value.Name));
                _presenter.ShowItemShort(entry.Key, entry.Value.Name, entry.Value.AttackBonus, entry.Value.ArmorBonus, users);
            }
        }

        /// <summary>
        /// Prints one line per ability with its holders.
        /// </summary>
        private void ListAbilities()
        {
            if (_world.Abilities.Count == 0)
            {
                _presenter.ShowEmptyList("abilities", "create ability");
                return;
            }
            foreach (KeyValuePair<string, Ability> entry in _world.Abilities.All)
            {
                string holders = string.Join(", ", _world.HoldersOfAbility(entry.Key).Select(h => h.Value.Name));
                _presenter.ShowAbilityShort(entry.Key, entry.Value.Name, entry.Value.DamageMultiplier, holders);
            }
        }

        /// <summary>
        /// Prints the full state of one character.
        /// </summary>
        /// <param name="entry">The character id and instance.</param>
        private void ShowCharacter(KeyValuePair<string, Character> entry)
        {
            Character c = entry.Value;
            List<string> items = new();
            foreach (Equipment item in c.Inventory.Items)
            {
                _world.Items.TryGetId(item, out string? itemId);
                items.Add($"[{itemId ?? "?"}] {item.Name} (+{item.AttackBonus} ATK, +{item.ArmorBonus} ARM)");
            }
            List<string> abilities = new();
            foreach (string abilityId in _world.BookOf(entry.Key))
            {
                Ability ability = _world.Abilities.GetById(abilityId);
                abilities.Add($"[{abilityId}] {ability.Name} (x{ability.DamageMultiplier})");
            }
            _presenter.ShowCharacterDetail(
                entry.Key, c.Name, c.Health, c.MaxHealth,
                c.BaseAttack, c.TotalAttack, c.BaseArmor, c.TotalArmor,
                c.IsDefending, c.IsDefeated, items, abilities);
        }

        /// <summary>
        /// Prints the full state of one item.
        /// </summary>
        /// <param name="entry">The item id and instance.</param>
        private void ShowItem(KeyValuePair<string, Equipment> entry)
        {
            string users = string.Join(", ", _world.UsersOfItem(entry.Value).Select(u => $"{u.Value.Name} [{u.Key}]"));
            _presenter.ShowItemDetail(entry.Key, entry.Value.Name, entry.Value.AttackBonus, entry.Value.ArmorBonus, users);
        }

        /// <summary>
        /// Prints the full state of one ability.
        /// </summary>
        /// <param name="entry">The ability id and instance.</param>
        private void ShowAbility(KeyValuePair<string, Ability> entry)
        {
            string holders = string.Join(", ", _world.HoldersOfAbility(entry.Key).Select(h => $"{h.Value.Name} [{h.Key}]"));
            _presenter.ShowAbilityDetail(entry.Key, entry.Value.Name, entry.Value.DamageMultiplier, holders);
        }
    }
}
