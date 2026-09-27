using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
namespace Cli.Chars
{
    /// <summary>
    /// Lists stored objects in short form, or one object in full detail.
    /// </summary>
    public sealed class LsCommand : IShellCommand
    {
        private readonly CharsWorld _world;

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
        /// Initializes a list command bound to the given world.
        /// </summary>
        /// <param name="world">The characters world.</param>
        public LsCommand(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            _world = world;
        }

        /// <summary>
        /// Prints the short category listing or the full object state.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage, "id");
            string category = command.RequireArg(0, "a category: ls <char|item|ability>.");
            string? selected = command.GetOption("id");

            switch (category.ToLowerInvariant())
            {
                case "char":
                    if (selected is null)
                        ListCharacters(console);
                    else
                        ShowCharacter(console, _world.ResolveCharacter(selected));
                    break;
                case "item":
                    if (selected is null)
                        ListItems(console);
                    else
                        ShowItem(console, _world.ResolveItem(selected));
                    break;
                case "ability":
                    if (selected is null)
                        ListAbilities(console);
                    else
                        ShowAbility(console, _world.ResolveAbility(selected));
                    break;
                default:
                    throw new CommandException($"Unknown category '{category}'. Use char, item or ability.");
            }
        }

        /// <summary>
        /// Prints one line per character.
        /// </summary>
        /// <param name="console">The console for output.</param>
        private void ListCharacters(IDisplay console)
        {
            if (_world.Characters.Count == 0)
            {
                console.WriteLine("No characters. Create one with: create char");
                return;
            }
            foreach (KeyValuePair<string, Character> entry in _world.Characters.All)
            {
                Character c = entry.Value;
                string state = c.IsDefeated ? " (defeated)" : "";
                console.WriteLine($"[{entry.Key}] {c.Name} - {c.Health}/{c.MaxHealth} HP, " +
                    $"{c.TotalAttack} ATK, {c.TotalArmor} ARM{state}");
            }
        }

        /// <summary>
        /// Prints one line per item with its users.
        /// </summary>
        /// <param name="console">The console for output.</param>
        private void ListItems(IDisplay console)
        {
            if (_world.Items.Count == 0)
            {
                console.WriteLine("No items. Create one with: create item");
                return;
            }
            foreach (KeyValuePair<string, Equipment> entry in _world.Items.All)
            {
                string users = string.Join(", ", _world.UsersOfItem(entry.Value).Select(u => u.Value.Name));
                console.WriteLine($"[{entry.Key}] {entry.Value.Name} " +
                    $"(+{entry.Value.AttackBonus} ATK, +{entry.Value.ArmorBonus} ARM) - used by: {Fallback(users)}");
            }
        }

        /// <summary>
        /// Prints one line per ability with its holders.
        /// </summary>
        /// <param name="console">The console for output.</param>
        private void ListAbilities(IDisplay console)
        {
            if (_world.Abilities.Count == 0)
            {
                console.WriteLine("No abilities. Create one with: create ability");
                return;
            }
            foreach (KeyValuePair<string, Ability> entry in _world.Abilities.All)
            {
                string holders = string.Join(", ", _world.HoldersOfAbility(entry.Key).Select(h => h.Value.Name));
                console.WriteLine($"[{entry.Key}] {entry.Value.Name} (x{entry.Value.DamageMultiplier}) - known by: {Fallback(holders)}");
            }
        }

        /// <summary>
        /// Prints the full state of one character.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="entry">The character id and instance.</param>
        private void ShowCharacter(IDisplay console, KeyValuePair<string, Character> entry)
        {
            Character c = entry.Value;
            console.WriteLine($"[{entry.Key}] {c.Name}");
            console.WriteLine($"  HP: {c.Health}/{c.MaxHealth}");
            console.WriteLine($"  Attack: {c.BaseAttack} base, {c.TotalAttack} total");
            console.WriteLine($"  Armor: {c.BaseArmor} base, {c.TotalArmor} total");
            console.WriteLine($"  Defending: {(c.IsDefending ? "yes" : "no")}");
            console.WriteLine($"  Defeated: {(c.IsDefeated ? "yes" : "no")}");

            if (c.Inventory.Count == 0)
            {
                console.WriteLine("  Items: -");
            }
            else
            {
                console.WriteLine("  Items:");
                foreach (Equipment item in c.Inventory.Items)
                {
                    _world.Items.TryGetId(item, out string? itemId);
                    console.WriteLine($"    [{itemId ?? "?"}] {item.Name} (+{item.AttackBonus} ATK, +{item.ArmorBonus} ARM)");
                }
            }

            IReadOnlyList<string> book = _world.BookOf(entry.Key);
            if (book.Count == 0)
            {
                console.WriteLine("  Abilities: -");
            }
            else
            {
                console.WriteLine("  Abilities:");
                foreach (string abilityId in book)
                {
                    Ability ability = _world.Abilities.GetById(abilityId);
                    console.WriteLine($"    [{abilityId}] {ability.Name} (x{ability.DamageMultiplier})");
                }
            }
        }

        /// <summary>
        /// Prints the full state of one item.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="entry">The item id and instance.</param>
        private void ShowItem(IDisplay console, KeyValuePair<string, Equipment> entry)
        {
            console.WriteLine($"[{entry.Key}] {entry.Value.Name}");
            console.WriteLine($"  Attack bonus: +{entry.Value.AttackBonus}");
            console.WriteLine($"  Armor bonus: +{entry.Value.ArmorBonus}");
            string users = string.Join(", ", _world.UsersOfItem(entry.Value).Select(u => $"{u.Value.Name} [{u.Key}]"));
            console.WriteLine($"  Used by: {Fallback(users)}");
        }

        /// <summary>
        /// Prints the full state of one ability.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="entry">The ability id and instance.</param>
        private void ShowAbility(IDisplay console, KeyValuePair<string, Ability> entry)
        {
            console.WriteLine($"[{entry.Key}] {entry.Value.Name}");
            console.WriteLine($"  Damage multiplier: x{entry.Value.DamageMultiplier}");
            string holders = string.Join(", ", _world.HoldersOfAbility(entry.Key).Select(h => $"{h.Value.Name} [{h.Key}]"));
            console.WriteLine($"  Known by: {Fallback(holders)}");
        }

        /// <summary>
        /// Replaces an empty user list with a dash.
        /// </summary>
        /// <param name="users">The comma joined user names.</param>
        /// <returns>The names, or "-" when empty.</returns>
        private static string Fallback(string users) =>
            string.IsNullOrEmpty(users) ? "-" : users;
    }
}
