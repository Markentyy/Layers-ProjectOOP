namespace Core.TextSystem
{
    /// <summary>
    /// Represents a container element that groups child elements under a title.
    /// Composite node of the document tree; navigation paths use section titles.
    /// </summary>
    public sealed class Section : TextElement
    {
        private readonly List<TextElement> _children = new();

        /// <summary>
        /// Gets the section title. Used as the path segment during navigation.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the child elements in their current order.
        /// </summary>
        public IReadOnlyList<TextElement> Children => _children;

        /// <summary>
        /// Gets the number of child elements.
        /// </summary>
        public int Count => _children.Count;

        /// <summary>
        /// Initializes a new section with the given title.
        /// </summary>
        /// <param name="title">Section title, also the navigation path segment.</param>
        public Section(string title)
        {
            ArgumentNullException.ThrowIfNull(title);
            Title = title;
        }

        /// <summary>
        /// Gets the section title for lookup by name.
        /// </summary>
        /// <returns>The section title.</returns>
        public override string DisplayName => Title;

        /// <summary>
        /// Appends a child element to the section.
        /// </summary>
        /// <param name="element">The child element to append.</param>
        public void AddChild(TextElement element)
        {
            ArgumentNullException.ThrowIfNull(element);
            _children.Add(element);
        }

        /// <summary>
        /// Removes a child element from the section.
        /// </summary>
        /// <param name="element">The child element to remove.</param>
        /// <returns>True when the element was found and removed.</returns>
        public bool RemoveChild(TextElement element) => _children.Remove(element);

        /// <summary>
        /// Renders the section title followed by all child elements.
        /// </summary>
        /// <returns>Formatted section string.</returns>
        public override string Render()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"\n# {Title}\n");
            foreach (TextElement child in _children)
            {
                sb.Append(child.Render());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Builds a table of contents entry for the section title.
        /// </summary>
        /// <returns>TOC entry text.</returns>
        public override string GetTableOfContentsEntry() => $"- {Title}";

        /// <summary>
        /// Appends the section entry and then recurses into children.
        /// </summary>
        /// <param name="entries">The entry sink.</param>
        /// <param name="depth">The nesting depth, used for indentation.</param>
        public override void CollectTableOfContents(List<string> entries, int depth)
        {
            string indent = new string(' ', depth * 2);
            entries.Add($"{indent}- {Title}");
            foreach (TextElement child in _children)
            {
                child.CollectTableOfContents(entries, depth + 1);
            }
        }
    }
}
