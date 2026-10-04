using Infra.Data;
using Infra.Display;

using Infra.WebApi;
namespace Cli.Presenter
{
    /// <summary>
    /// Formats all characters mode output: creation reports, combat lines,
    /// listings and detail views. Commands decide what happens, this class
    /// decides how it looks.
    /// </summary>
    public sealed class CharsPresenter
    {
        private readonly IDisplay _display;

        /// <summary>
        /// Initializes a presenter writing to the given display.
        /// </summary>
        /// <param name="display">The display for output.</param>
        public CharsPresenter(IDisplay display)
        {
            ArgumentNullException.ThrowIfNull(display);
            _display = display;
        }

        /// <summary>
        /// Reports a created character.
        /// </summary>
        /// <param name="id">The assigned id.</param>
        /// <param name="name">The character name.</param>
        /// <param name="maxHealth">The maximum health.</param>
        /// <param name="armor">The base armor.</param>
        /// <param name="attack">The base attack.</param>
        public void ShowCharacterCreated(string id, string name, int maxHealth, int armor, int attack) =>
            _display.WriteLine($"Created character '{name}' [{id}] ({maxHealth} HP, {armor} ARM, {attack} ATK).");

        /// <summary>
        /// Reports a created item.
        /// </summary>
        /// <param name="id">The assigned id.</param>
        /// <param name="name">The item name.</param>
        /// <param name="attack">The attack bonus.</param>
        /// <param name="armor">The armor bonus.</param>
        public void ShowItemCreated(string id, string name, int attack, int armor) =>
            _display.WriteLine($"Created item '{name}' [{id}] (+{attack} ATK, +{armor} ARM).");

        /// <summary>
        /// Reports a created ability.
        /// </summary>
        /// <param name="id">The assigned id.</param>
        /// <param name="name">The ability name.</param>
        /// <param name="multiplier">The damage multiplier.</param>
        public void ShowAbilityCreated(string id, string name, int multiplier) =>
            _display.WriteLine($"Created ability '{name}' [{id}] (x{multiplier}).");

        /// <summary>
        /// Reports an equipped item.
        /// </summary>
        /// <param name="charName">The character name.</param>
        /// <param name="itemName">The item name.</param>
        /// <param name="attack">The attack bonus.</param>
        /// <param name="armor">The armor bonus.</param>
        public void ShowEquipped(string charName, string itemName, int attack, int armor) =>
            _display.WriteLine($"{charName} equipped {itemName} (+{attack} ATK, +{armor} ARM).");

        /// <summary>
        /// Reports an already equipped item.
        /// </summary>
        /// <param name="charName">The character name.</param>
        /// <param name="itemName">The item name.</param>
        public void ShowAlreadyEquipped(string charName, string itemName) =>
            _display.WriteLine($"{charName} already has {itemName} equipped.");

        /// <summary>
        /// Reports a learned ability.
        /// </summary>
        /// <param name="charName">The character name.</param>
        /// <param name="abilityName">The ability name.</param>
        /// <param name="abilityId">The ability id.</param>
        public void ShowLearned(string charName, string abilityName, string abilityId) =>
            _display.WriteLine($"{charName} learned {abilityName} [{abilityId}].");

        /// <summary>
        /// Reports an already known ability.
        /// </summary>
        /// <param name="charName">The character name.</param>
        /// <param name="abilityName">The ability name.</param>
        public void ShowAlreadyKnows(string charName, string abilityName) =>
            _display.WriteLine($"{charName} already knows {abilityName}.");

        /// <summary>
        /// Reports a standard attack.
        /// </summary>
        /// <param name="attacker">The attacker name.</param>
        /// <param name="target">The target name.</param>
        /// <param name="damage">The damage dealt.</param>
        public void ShowAttack(string attacker, string target, int damage) =>
            _display.WriteLine($"{attacker} attacks {target} for {damage} damage!");

        /// <summary>
        /// Announces a defeat.
        /// </summary>
        /// <param name="name">The defeated character name.</param>
        public void ShowDefeat(string name) =>
            _display.WriteLine($"{name} has been defeated!");

        /// <summary>
        /// Reports healing.
        /// </summary>
        /// <param name="name">The healed character name.</param>
        /// <param name="amount">The restored amount.</param>
        /// <param name="health">The current health.</param>
        /// <param name="maxHealth">The maximum health.</param>
        public void ShowHeal(string name, int amount, int health, int maxHealth) =>
            _display.WriteLine($"{name} heals for {amount} HP. Current HP: {health}/{maxHealth}");

        /// <summary>
        /// Reports an ability use.
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="ability">The ability name.</param>
        /// <param name="target">The target name.</param>
        /// <param name="damage">The damage dealt.</param>
        public void ShowAbilityUse(string user, string ability, string target, int damage)
        {
            _display.WriteLine($"{user} uses special ability: [{ability}] on {target}!");
            _display.WriteLine($"It deals {damage} damage!");
        }

        /// <summary>
        /// Prints one short character line.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="name">The character name.</param>
        /// <param name="health">The current health.</param>
        /// <param name="maxHealth">The maximum health.</param>
        /// <param name="attack">The total attack.</param>
        /// <param name="armor">The total armor.</param>
        /// <param name="defeated">Whether the character is defeated.</param>
        public void ShowCharacterShort(string id, string name, int health, int maxHealth, int attack, int armor, bool defeated)
        {
            string state = defeated ? " (defeated)" : "";
            _display.WriteLine($"[{id}] {name} - {health}/{maxHealth} HP, {attack} ATK, {armor} ARM{state}");
        }

        /// <summary>
        /// Prints one short item line with its users.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="name">The item name.</param>
        /// <param name="attack">The attack bonus.</param>
        /// <param name="armor">The armor bonus.</param>
        /// <param name="users">The user names, possibly empty.</param>
        public void ShowItemShort(string id, string name, int attack, int armor, string users) =>
            _display.WriteLine($"[{id}] {name} (+{attack} ATK, +{armor} ARM) - used by: {Fallback(users)}");

        /// <summary>
        /// Prints one short ability line with its holders.
        /// </summary>
        /// <param name="id">The ability id.</param>
        /// <param name="name">The ability name.</param>
        /// <param name="multiplier">The damage multiplier.</param>
        /// <param name="holders">The holder names, possibly empty.</param>
        public void ShowAbilityShort(string id, string name, int multiplier, string holders) =>
            _display.WriteLine($"[{id}] {name} (x{multiplier}) - known by: {Fallback(holders)}");

        /// <summary>
        /// Prints the empty registry hint.
        /// </summary>
        /// <param name="category">The plural category name.</param>
        /// <param name="createHint">The create command to suggest.</param>
        public void ShowEmptyList(string category, string createHint) =>
            _display.WriteLine($"No {category}. Create one with: {createHint}");

        /// <summary>
        /// Prints the full character state.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="name">The character name.</param>
        /// <param name="health">The current health.</param>
        /// <param name="maxHealth">The maximum health.</param>
        /// <param name="baseAttack">The base attack.</param>
        /// <param name="totalAttack">The total attack.</param>
        /// <param name="baseArmor">The base armor.</param>
        /// <param name="totalArmor">The total armor.</param>
        /// <param name="defending">Whether defending.</param>
        /// <param name="defeated">Whether defeated.</param>
        /// <param name="items">The carried items as preformatted lines.</param>
        /// <param name="abilities">The known abilities as preformatted lines.</param>
        public void ShowCharacterDetail(
            string id, string name, int health, int maxHealth,
            int baseAttack, int totalAttack, int baseArmor, int totalArmor,
            bool defending, bool defeated,
            IReadOnlyList<string> items, IReadOnlyList<string> abilities)
        {
            _display.WriteLine($"[{id}] {name}");
            _display.WriteLine($"  HP: {health}/{maxHealth}");
            _display.WriteLine($"  Attack: {baseAttack} base, {totalAttack} total");
            _display.WriteLine($"  Armor: {baseArmor} base, {totalArmor} total");
            _display.WriteLine($"  Defending: {(defending ? "yes" : "no")}");
            _display.WriteLine($"  Defeated: {(defeated ? "yes" : "no")}");
            if (items.Count == 0)
                _display.WriteLine("  Items: -");
            else
            {
                _display.WriteLine("  Items:");
                foreach (string line in items)
                    _display.WriteLine($"    {line}");
            }
            if (abilities.Count == 0)
                _display.WriteLine("  Abilities: -");
            else
            {
                _display.WriteLine("  Abilities:");
                foreach (string line in abilities)
                    _display.WriteLine($"    {line}");
            }
        }

        /// <summary>
        /// Prints the full item state.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="name">The item name.</param>
        /// <param name="attack">The attack bonus.</param>
        /// <param name="armor">The armor bonus.</param>
        /// <param name="users">The user names with ids, possibly empty.</param>
        public void ShowItemDetail(string id, string name, int attack, int armor, string users)
        {
            _display.WriteLine($"[{id}] {name}");
            _display.WriteLine($"  Attack bonus: +{attack}");
            _display.WriteLine($"  Armor bonus: +{armor}");
            _display.WriteLine($"  Used by: {Fallback(users)}");
        }

        /// <summary>
        /// Prints the full ability state.
        /// </summary>
        /// <param name="id">The ability id.</param>
        /// <param name="name">The ability name.</param>
        /// <param name="multiplier">The damage multiplier.</param>
        /// <param name="holders">The holder names with ids, possibly empty.</param>
        public void ShowAbilityDetail(string id, string name, int multiplier, string holders)
        {
            _display.WriteLine($"[{id}] {name}");
            _display.WriteLine($"  Damage multiplier: x{multiplier}");
            _display.WriteLine($"  Known by: {Fallback(holders)}");
        }

        /// <summary>
        /// Replaces an empty name list with a dash.
        /// </summary>
        /// <param name="users">The comma joined names.</param>
        /// <returns>The names, or "-" when empty.</returns>
        private static string Fallback(string users) =>
            string.IsNullOrEmpty(users) ? "-" : users;

        /// <summary>
        /// Reports a saved world file.
        /// </summary>
        /// <param name="path">The destination file path.</param>
        public void ShowSaved(string path) =>
            _display.WriteLine($"Saved to '{path}'.");

        /// <summary>
        /// Reports a loaded world file with object counts.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <param name="characters">The loaded character count.</param>
        /// <param name="items">The loaded item count.</param>
        /// <param name="abilities">The loaded ability count.</param>
        public void ShowLoaded(string path, int characters, int items, int abilities) =>
            _display.WriteLine($"Loaded from '{path}': {characters} characters, {items} items, {abilities} abilities.");

        /// <summary>
        /// Prints one database character line.
        /// </summary>
        /// <param name="id">The database id.</param>
        /// <param name="character">The database character.</param>
        public void ShowDbCharacter(string id, GenshinCharacterDto character) =>
            _display.WriteLine($"[{id}] {character.Name} ({character.Rarity} stars, {character.Vision} {character.Weapon}, {character.Nation})");

        /// <summary>
        /// Prints one database weapon line.
        /// </summary>
        /// <param name="id">The database id.</param>
        /// <param name="weapon">The database weapon.</param>
        public void ShowDbWeapon(string id, GenshinWeaponDto weapon) =>
            _display.WriteLine($"[{id}] {weapon.Name} ({weapon.Rarity} stars {weapon.Type}, ATK {weapon.BaseAttack})");

        /// <summary>
        /// Reports a recruited database hero.
        /// </summary>
        /// <param name="name">The hero name.</param>
        /// <param name="id">The assigned id.</param>
        /// <param name="maxHealth">The rolled health.</param>
        /// <param name="armor">The rolled armor.</param>
        /// <param name="attack">The rolled attack.</param>
        /// <param name="talents">The learned talent count.</param>
        public void ShowRecruited(string name, string id, int maxHealth, int armor, int attack, int talents) =>
            _display.WriteLine($"Recruited '{name}' [{id}] ({maxHealth} HP, {armor} ARM, {attack} ATK, {talents} talents learned).");

        /// <summary>
        /// Reports a fetched database weapon.
        /// </summary>
        /// <param name="name">The weapon name.</param>
        /// <param name="id">The assigned id.</param>
        /// <param name="attack">The attack bonus.</param>
        /// <param name="armor">The armor bonus.</param>
        public void ShowFetched(string name, string id, int attack, int armor) =>
            _display.WriteLine($"Fetched '{name}' [{id}] (+{attack} ATK, +{armor} ARM).");

        /// <summary>
        /// Writes a pre-rendered character sheet as is.
        /// </summary>
        /// <param name="text">The rendered sheet text.</param>
        public void ShowSheet(string text) => _display.Write(text);
    }
}
