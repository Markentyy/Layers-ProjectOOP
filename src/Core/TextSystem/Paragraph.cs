namespace Core.TextSystem
{
    /// <summary>
    /// Represents a standard paragraph of text.
    /// </summary>
    public sealed class Paragraph : TextElement
    {
        /// <summary>
        /// Gets the paragraph content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Initializes a new paragraph with the given content.
        /// </summary>
        /// <param name="content">Paragraph text.</param>
        public Paragraph(string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            Content = content;
        }

        /// <summary>
        /// Gets the paragraph content for lookup by name.
        /// </summary>
        /// <returns>The paragraph content.</returns>
        public override string DisplayName => Content;

        /// <summary>
        /// Renders the paragraph as plain text with a trailing newline.
        /// </summary>
        /// <returns>Formatted paragraph string.</returns>
        public override string Render() => $"{Content}\n";
    }
}
