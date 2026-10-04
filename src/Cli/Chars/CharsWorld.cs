using Core.GameSystem;
using Infra.Data;
using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// The characters interpreter state: three registries plus ability books.
    /// Characters learn abilities through the book; items are shared by reference.
    /// </summary>
    public sealed class CharsWorld
    {
        /// <summary>
        /// Gets the character registry (auto ids "char-N").
        /// </summary>
        public Registry<Character> Characters { get; } =
            new("char", c => c.Name);

        /// <summary>
        /// Gets the item registry (auto ids "item-N").
        /// </summary>
        public Registry<Equipment> Items { get; } =
            new("item", i => i.Name);

        /// <summary>
        /// Gets the ability registry (auto ids "ability-N").
        /// </summary>
        public Registry<Ability> Abilities { get; } =
            new("ability", a => a.Name);

        private readonly Dictionary<string, List<string>> _books =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the remote lore by character id for heroes recruited from the database.
        /// </summary>
        public Dictionary<string, GenshinCharacterDto> Lore { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the ability ids known by the given character id.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <returns>The known ability ids.</returns>
        public IReadOnlyList<string> BookOf(string characterId) =>
            _books.TryGetValue(characterId, out List<string>? book)
                ? book
                : Array.Empty<string>();

        /// <summary>
        /// Attaches an ability to a character book. Already known abilities are ignored.
        /// </summary>
        /// <param name="characterId">The character id.</param>
        /// <param name="abilityId">The ability id.</param>
        /// <returns>True when newly learned, false when already known.</returns>
        public bool Learn(string characterId, string abilityId)
        {
            if (!_books.TryGetValue(characterId, out List<string>? book))
            {
                book = new List<string>();
                _books.Add(characterId, book);
            }
            if (book.Contains(abilityId, StringComparer.OrdinalIgnoreCase))
                return false;
            book.Add(abilityId);
            return true;
        }

        /// <summary>
        /// Removes every character, item, ability and book entry.
        /// </summary>
        public void Clear()
        {
            Characters.Clear();
            Items.Clear();
            Abilities.Clear();
            _books.Clear();
            Lore.Clear();
        }

        /// <summary>
        /// Checks whether the id is taken in any registry.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True when the id is already used.</returns>
        public bool IsIdTaken(string id) =>
            Characters.ContainsId(id) || Items.ContainsId(id) || Abilities.ContainsId(id);

        /// <summary>
        /// Adds a character, keeping its id unique across all registries.
        /// </summary>
        /// <param name="character">The character to store.</param>
        /// <param name="customId">The requested id, or null for an auto id.</param>
        /// <returns>The assigned id.</returns>
        public string AddCharacter(Character character, string? customId = null)
        {
            RequireFreeId(customId);
            return Characters.Add(character, customId);
        }

        /// <summary>
        /// Adds an item, keeping its id unique across all registries.
        /// </summary>
        /// <param name="item">The item to store.</param>
        /// <param name="customId">The requested id, or null for an auto id.</param>
        /// <returns>The assigned id.</returns>
        public string AddItem(Equipment item, string? customId = null)
        {
            RequireFreeId(customId);
            return Items.Add(item, customId);
        }

        /// <summary>
        /// Adds an ability, keeping its id unique across all registries.
        /// </summary>
        /// <param name="ability">The ability to store.</param>
        /// <param name="customId">The requested id, or null for an auto id.</param>
        /// <returns>The assigned id.</returns>
        public string AddAbility(Ability ability, string? customId = null)
        {
            RequireFreeId(customId);
            return Abilities.Add(ability, customId);
        }

        /// <summary>
        /// Rejects a custom id that is already used anywhere.
        /// </summary>
        /// <param name="customId">The requested id, or null.</param>
        private void RequireFreeId(string? customId)
        {
            if (!string.IsNullOrWhiteSpace(customId) && IsIdTaken(customId.Trim()))
                throw new Engine.CommandException($"Id '{customId.Trim()}' is already taken.");
        }

        /// <summary>
        /// Resolves a character by id or name.
        /// </summary>
        /// <param name="idOrName">The id or the name.</param>
        /// <returns>The character id and instance.</returns>
        public KeyValuePair<string, Character> ResolveCharacter(string idOrName) =>
            Resolve(Characters, "character", idOrName);

        /// <summary>
        /// Resolves an item by id or name.
        /// </summary>
        /// <param name="idOrName">The id or the name.</param>
        /// <returns>The item id and instance.</returns>
        public KeyValuePair<string, Equipment> ResolveItem(string idOrName) =>
            Resolve(Items, "item", idOrName);

        /// <summary>
        /// Resolves an ability by id or name.
        /// </summary>
        /// <param name="idOrName">The id or the name.</param>
        /// <returns>The ability id and instance.</returns>
        public KeyValuePair<string, Ability> ResolveAbility(string idOrName) =>
            Resolve(Abilities, "ability", idOrName);

        /// <summary>
        /// Finds characters carrying the given item instance.
        /// </summary>
        /// <param name="item">The item instance.</param>
        /// <returns>Character ids and instances using it.</returns>
        public IEnumerable<KeyValuePair<string, Character>> UsersOfItem(Equipment item)
        {
            foreach (KeyValuePair<string, Character> entry in Characters.All)
            {
                if (entry.Value.Inventory.Items.Contains(item))
                    yield return entry;
            }
        }

        /// <summary>
        /// Finds characters knowing the given ability id.
        /// </summary>
        /// <param name="abilityId">The ability id.</param>
        /// <returns>Character ids and instances knowing it.</returns>
        public IEnumerable<KeyValuePair<string, Character>> HoldersOfAbility(string abilityId)
        {
            foreach (KeyValuePair<string, Character> entry in Characters.All)
            {
                if (BookOf(entry.Key).Contains(abilityId, StringComparer.OrdinalIgnoreCase))
                    yield return entry;
            }
        }

        /// <summary>
        /// Resolves one registry entry by id first, then by name.
        /// </summary>
        /// <typeparam name="T">The stored type.</typeparam>
        /// <param name="registry">The registry to search.</param>
        /// <param name="kind">The kind name used in error texts.</param>
        /// <param name="idOrName">The id or the name.</param>
        /// <returns>The entry id and value.</returns>
        private static KeyValuePair<string, T> Resolve<T>(Registry<T> registry, string kind, string idOrName)
            where T : class
        {
            if (registry.ContainsId(idOrName))
                return new KeyValuePair<string, T>(idOrName, registry.GetById(idOrName));

            IReadOnlyList<string> ids = registry.IdsOfName(idOrName);
            if (ids.Count == 1)
                return new KeyValuePair<string, T>(ids[0], registry.GetById(ids[0]));
            if (ids.Count == 0)
                throw new Engine.CommandException($"Unknown {kind} '{idOrName}'.");
            throw new Engine.CommandException(
                $"Ambiguous {kind} name '{idOrName}': {string.Join(", ", ids)}. Use an id.");
        }
    }
}
