using System.Text;

namespace Core.TextSystem
{
    /// <summary>
    /// The root container that manages a collection of text elements.
    /// Works only through the abstract element contract, never through concrete types.
    /// </summary>
    public sealed class TextDocument
    {
        private readonly List<TextElement> _elements = new();

        /// <summary>
        /// Gets the number of elements in the document.
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// Gets the elements in their current order.
        /// </summary>
        public IReadOnlyList<TextElement> Elements => _elements;

        /// <summary>
        /// Adds a new element to the document.
        /// </summary>
        /// <param name="element">The text element to append.</param>
        public void AddElement(TextElement element)
        {
            ArgumentNullException.ThrowIfNull(element);
            _elements.Add(element);
        }

        /// <summary>
        /// Removes a specific element from the document.
        /// </summary>
        /// <param name="element">The text element to remove.</param>
        /// <returns>True when the element was found and removed.</returns>
        public bool RemoveElement(TextElement element) => _elements.Remove(element);

        /// <summary>
        /// Reorders an element by moving it to a new index.
        /// </summary>
        /// <param name="currentIndex">Current position of the element.</param>
        /// <param name="newIndex">Target position of the element.</param>
        public void MoveElement(int currentIndex, int newIndex)
        {
            if (currentIndex < 0 || currentIndex >= _elements.Count || newIndex < 0 || newIndex >= _elements.Count)
                return;

            TextElement item = _elements[currentIndex];
            _elements.RemoveAt(currentIndex);
            _elements.Insert(newIndex, item);
        }

        /// <summary>
        /// Removes all elements from the document.
        /// </summary>
        public void Clear() => _elements.Clear();

        /// <summary>
        /// Renders the entire document into a single formatted string.
        /// </summary>
        /// <returns>Full rendered document text.</returns>
        public string RenderDocument()
        {
            StringBuilder sb = new();
            foreach (TextElement element in _elements)
            {
                sb.Append(element.Render());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Extracts and formats all headings into a Table of Contents.
        /// Each element contributes its own entries, sections recurse into children.
        /// </summary>
        /// <returns>Formatted table of contents text.</returns>
        public string RenderTableOfContents()
        {
            StringBuilder sb = new();
            sb.AppendLine("--- Table of Contents ---");

            List<string> entries = new();
            foreach (TextElement element in _elements)
            {
                element.CollectTableOfContents(entries, 0);
            }
            foreach (string entry in entries)
            {
                sb.AppendLine(entry);
            }
            return sb.ToString();
        }
    }
}
