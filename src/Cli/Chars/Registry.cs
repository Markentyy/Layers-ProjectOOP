namespace Cli.Chars
{
    /// <summary>
    /// Named store for one object category with unique string identifiers.
    /// Lookup works by id or by name; duplicate names are reported as ambiguous.
    /// </summary>
    /// <typeparam name="T">The stored reference type.</typeparam>
    public sealed class Registry<T>
        where T : class
    {
        private readonly Dictionary<string, T> _byId =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<T, string> _ids =
            new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<string, List<string>> _nameIndex =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Func<T, string> _nameOf;
        private readonly string _prefix;
        private int _counter;

        /// <summary>
        /// Initializes a registry with an id prefix and a name selector.
        /// </summary>
        /// <param name="prefix">The prefix for generated ids (e.g. "char").</param>
        /// <param name="nameOf">Selects the lookup name of a stored object.</param>
        public Registry(string prefix, Func<T, string> nameOf)
        {
            ArgumentNullException.ThrowIfNull(nameOf);
            _prefix = prefix;
            _nameOf = nameOf;
        }

        /// <summary>
        /// Gets all stored objects with their ids in insertion order.
        /// </summary>
        public IEnumerable<KeyValuePair<string, T>> All => _byId;

        /// <summary>
        /// Gets the number of stored objects.
        /// </summary>
        public int Count => _byId.Count;

        /// <summary>
        /// Adds an object, using the custom id when provided and free.
        /// </summary>
        /// <param name="item">The object to store.</param>
        /// <param name="customId">The requested id, or null for an auto id.</param>
        /// <returns>The assigned id.</returns>
        public string Add(T item, string? customId = null)
        {
            ArgumentNullException.ThrowIfNull(item);
            string id;
            if (!string.IsNullOrWhiteSpace(customId))
            {
                id = customId.Trim();
                if (_byId.ContainsKey(id))
                    throw new Engine.CommandException($"Id '{id}' is already taken.");
            }
            else
            {
                do
                {
                    _counter++;
                    id = $"{_prefix}-{_counter}";
                }
                while (_byId.ContainsKey(id));
            }

            _byId.Add(id, item);
            _ids.Add(item, id);
            string name = _nameOf(item);
            if (!_nameIndex.TryGetValue(name, out List<string>? ids))
            {
                ids = new List<string>();
                _nameIndex.Add(name, ids);
            }
            ids.Add(id);
            return id;
        }

        /// <summary>
        /// Checks whether the given id is registered.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True when present.</returns>
        public bool ContainsId(string id) => _byId.ContainsKey(id);

        /// <summary>
        /// Gets the object stored under the given id.
        /// </summary>
        /// <param name="id">The object id.</param>
        /// <returns>The stored object.</returns>
        public T GetById(string id) => _byId[id];

        /// <summary>
        /// Finds the id assigned to the given stored instance.
        /// </summary>
        /// <param name="item">The stored instance.</param>
        /// <param name="id">The found id, or null.</param>
        /// <returns>True when the instance is registered.</returns>
        public bool TryGetId(T item, out string? id) => _ids.TryGetValue(item, out id);

        /// <summary>
        /// Gets the ids of all objects stored under the given name.
        /// </summary>
        /// <param name="name">The lookup name.</param>
        /// <returns>The matching ids, possibly empty.</returns>
        public IReadOnlyList<string> IdsOfName(string name) =>
            _nameIndex.TryGetValue(name, out List<string>? ids) ? ids : Array.Empty<string>();
    }
}
