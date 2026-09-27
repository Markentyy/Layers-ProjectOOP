namespace Core.TextSystem
{
    /// <summary>
    /// Abstract base for every structured text element.
    /// Defines the shared contract and hides rendering details from clients.
    /// </summary>
    public abstract class TextElement : ITextElement
    {
        /// <summary>
        /// Gets or sets the unique identifier of the element within a document.
        /// </summary>
        /// <remarks>
        /// Assigned by the owning layer (document explorer), not by the element itself.
        /// Empty until the element is attached to a numbered document.
        /// </remarks>
        public string Id { get; internal set; } = "";

        /// <summary>
        /// Gets the human readable name used for lookup by name (title, text or content).
        /// </summary>
        /// <returns>The display name of the element.</returns>
        public virtual string DisplayName => "";

        /// <summary>
        /// Renders the element to its string representation.
        /// </summary>
        /// <returns>Formatted string content.</returns>
        public abstract string Render();

        /// <summary>
        /// Builds a table of contents entry for this element,
        /// or null when the element does not belong in the table.
        /// </summary>
        /// <returns>TOC entry text, or null when not applicable.</returns>
        public virtual string? GetTableOfContentsEntry()
        {
            return null;
        }

        /// <summary>
        /// Appends table of contents entries of this element and its children.
        /// </summary>
        /// <param name="entries">The entry sink.</param>
        /// <param name="depth">The nesting depth, used for indentation.</param>
        public virtual void CollectTableOfContents(List<string> entries, int depth)
        {
        }
    }
}
