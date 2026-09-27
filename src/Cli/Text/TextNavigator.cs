using Core.TextSystem;

namespace Cli.Text
{
    /// <summary>
    /// The text interpreter state: a document plus the current position in it.
    /// Owns element numbering and all navigation between containers.
    /// </summary>
    public sealed class TextNavigator
    {
        private TextDocument _document;
        private readonly Stack<Section> _path = new();
        private int _idCounter;

        /// <summary>
        /// Initializes a navigator over the given document and numbers its elements.
        /// </summary>
        /// <param name="document">The document to explore.</param>
        public TextNavigator(TextDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            _document = document;
            _idCounter = MaxUsedId(document.Elements);
            EnsureIds(_document.Elements);
        }

        /// <summary>
        /// Replaces the explored document, resets the position to the root
        /// and continues numbering after the loaded ids.
        /// </summary>
        /// <param name="document">The document to explore.</param>
        public void ReplaceDocument(TextDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            _document = document;
            _path.Clear();
            _idCounter = MaxUsedId(document.Elements);
            EnsureIds(document.Elements);
        }

        /// <summary>
        /// Gets the explored document.
        /// </summary>
        public TextDocument Document => _document;

        /// <summary>
        /// Gets whether the current position is the document root.
        /// </summary>
        public bool IsRoot => _path.Count == 0;

        /// <summary>
        /// Gets the children of the current container in order.
        /// </summary>
        public IReadOnlyList<TextElement> CurrentChildren =>
            IsRoot ? _document.Elements : _path.Peek().Children;

        /// <summary>
        /// Gets the title of the current container, or an empty string at the root.
        /// </summary>
        public string CurrentTitle => IsRoot ? "" : _path.Peek().Title;

        /// <summary>
        /// Gets the current path in the form /section/subsection, or / at the root.
        /// </summary>
        /// <returns>The current path.</returns>
        public string Pwd()
        {
            if (IsRoot)
                return "/";
            return "/" + string.Join("/", _path.Reverse().Select(s => s.Title));
        }

        /// <summary>
        /// Moves into the container addressed by the given path.
        /// Absolute paths start with /, ".." goes up, names match section titles.
        /// </summary>
        /// <param name="path">The path to follow.</param>
        public void Cd(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            Stack<Section> next = new(_path.Reverse());
            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                next.Clear();
            }

            foreach (string raw in path.Split('/'))
            {
                string segment = raw.Trim();
                if (segment.Length == 0 || segment == ".")
                    continue;
                if (segment == "..")
                {
                    if (next.Count == 0)
                        throw new Engine.CommandException("Already at the document root.");
                    next.Pop();
                    continue;
                }

                Section? child = ChildrenOf(next).OfType<Section>()
                    .FirstOrDefault(s => s.Title.Equals(segment, StringComparison.OrdinalIgnoreCase));
                if (child is null)
                    throw new Engine.CommandException($"No section '{segment}' here.");
                next.Push(child);
            }

            _path.Clear();
            foreach (Section section in next.Reverse())
            {
                _path.Push(section);
            }
        }

        /// <summary>
        /// Jumps directly to the container with the given id.
        /// </summary>
        /// <param name="id">The container id.</param>
        public void CdById(string id)
        {
            if (!TryFind(id, out TextElement? element, out List<Section> trail) || element is null)
                throw new Engine.CommandException($"Unknown element '{id}'.");
            if (element is not Section section)
                throw new Engine.CommandException($"Element '{id}' is not a container.");
            trail.Add(section);
            _path.Clear();
            foreach (Section step in trail)
            {
                _path.Push(step);
            }
        }

        /// <summary>
        /// Moves to the parent of the current container.
        /// </summary>
        public void Up()
        {
            if (IsRoot)
                throw new Engine.CommandException("Already at the document root.");
            _path.Pop();
        }

        /// <summary>
        /// Finds a direct child of the current container by display name.
        /// </summary>
        /// <param name="name">The title, text, content or display text.</param>
        /// <returns>The found elements, possibly empty.</returns>
        public IReadOnlyList<TextElement> FindInCurrent(string name) =>
            CurrentChildren
                .Where(e => e.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

        /// <summary>
        /// Adds an element to the current container and numbers it.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void AddToCurrent(TextElement element)
        {
            ArgumentNullException.ThrowIfNull(element);
            if (IsRoot)
                _document.AddElement(element);
            else
                _path.Peek().AddChild(element);
            EnsureIds(_document.Elements);
        }

        /// <summary>
        /// Removes a direct child from the current container.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void RemoveFromCurrent(TextElement element)
        {
            ArgumentNullException.ThrowIfNull(element);
            bool removed = IsRoot
                ? _document.RemoveElement(element)
                : _path.Peek().RemoveChild(element);
            if (!removed)
                throw new Engine.CommandException("Element is no longer here.");
        }

        /// <summary>
        /// Removes the current section itself and moves to its parent.
        /// </summary>
        /// <returns>The removed section.</returns>
        public Section RemoveCurrent()
        {
            if (IsRoot)
                throw new Engine.CommandException("Nothing selected: the current position is the document root.");
            Section current = _path.Pop();
            if (_path.Count == 0)
                _document.RemoveElement(current);
            else
                _path.Peek().RemoveChild(current);
            return current;
        }

        /// <summary>
        /// Renders the current container children.
        /// </summary>
        /// <param name="showIds">Prefix every element with its id line.</param>
        /// <returns>The rendered text.</returns>
        public string RenderCurrent(bool showIds) =>
            RenderMany(CurrentChildren, showIds);

        /// <summary>
        /// Renders the whole document from the root.
        /// </summary>
        /// <param name="showIds">Prefix every element with its id line.</param>
        /// <returns>The rendered text.</returns>
        public string RenderWhole(bool showIds) =>
            RenderMany(_document.Elements, showIds);

        /// <summary>
        /// Renders a flat element list, optionally with id marker lines.
        /// </summary>
        /// <param name="elements">The elements to render.</param>
        /// <param name="showIds">Prefix every element with its id line.</param>
        /// <returns>The rendered text.</returns>
        private static string RenderMany(IReadOnlyList<TextElement> elements, bool showIds)
        {
            var sb = new System.Text.StringBuilder();
            foreach (TextElement element in elements)
            {
                if (showIds)
                    sb.AppendLine($"[{element.Id}] {element.DisplayName} ({element.GetType().Name})");
                sb.Append(element.Render());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Assigns ids to every element that has none yet, depth first.
        /// </summary>
        /// <param name="elements">The elements to number.</param>
        private void EnsureIds(IEnumerable<TextElement> elements)
        {
            foreach (TextElement element in elements)
            {
                if (string.IsNullOrEmpty(element.Id))
                    element.Id = $"e{++_idCounter}";
                if (element is Section section)
                    EnsureIds(section.Children);
            }
        }

        /// <summary>
        /// Finds the highest numeric suffix of stored e-number ids.
        /// </summary>
        /// <param name="elements">The elements to scan.</param>
        /// <returns>The maximum suffix, or zero when none.</returns>
        private static int MaxUsedId(IEnumerable<TextElement> elements)
        {
            int max = 0;
            foreach (TextElement element in elements)
            {
                if (element.Id.StartsWith("e", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(element.Id[1..], out int n) && n > max)
                    max = n;
                if (element is Section section)
                    max = Math.Max(max, MaxUsedId(section.Children));
            }
            return max;
        }

        /// <summary>
        /// Gets the children of the container addressed by the given stack.
        /// </summary>
        /// <param name="stack">The navigation stack, root first.</param>
        /// <returns>The child elements.</returns>
        private IReadOnlyList<TextElement> ChildrenOf(Stack<Section> stack) =>
            stack.Count == 0 ? _document.Elements : stack.Peek().Children;

        /// <summary>
        /// Finds the element with the given id and its parent section trail.
        /// </summary>
        /// <param name="id">The element id.</param>
        /// <param name="element">The found element, or null.</param>
        /// <param name="trail">The parent sections from the root.</param>
        /// <returns>True when the element exists.</returns>
        private bool TryFind(string id, out TextElement? element, out List<Section> trail)
        {
            element = null;
            trail = new List<Section>();
            return FindRecursive(_document.Elements, id, trail, ref element);
        }

        /// <summary>
        /// Depth first search carrying the parent section trail.
        /// </summary>
        /// <param name="elements">The elements to search.</param>
        /// <param name="id">The wanted id.</param>
        /// <param name="trail">The parent sections of the searched level.</param>
        /// <param name="element">The match, or null.</param>
        /// <returns>True on match.</returns>
        private static bool FindRecursive(
            IEnumerable<TextElement> elements,
            string id,
            List<Section> trail,
            ref TextElement? element)
        {
            foreach (TextElement candidate in elements)
            {
                if (candidate.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    element = candidate;
                    return true;
                }
                if (candidate is Section child)
                {
                    trail.Add(child);
                    if (FindRecursive(child.Children, id, trail, ref element))
                        return true;
                    trail.RemoveAt(trail.Count - 1);
                }
            }
            return false;
        }
    }
}
