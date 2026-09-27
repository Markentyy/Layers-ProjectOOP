namespace Core.TextSystem
{
    /// <summary>
    /// Fluent builder that owns the step by step construction of a structured document.
    /// Clients use one small unified API instead of combining elements and the document by hand.
    /// </summary>
    public sealed class DocumentBuilder
    {
        private readonly TextDocument _document = new();

        /// <summary>
        /// Gets the number of elements added so far.
        /// </summary>
        public int Count => _document.Count;

        /// <summary>
        /// Creates a heading and appends it to the document.
        /// </summary>
        /// <param name="level">Heading level from 1 to 6.</param>
        /// <param name="text">Heading text.</param>
        /// <returns>This builder for chaining.</returns>
        public DocumentBuilder AddHeading(int level, string text)
        {
            _document.AddElement(new Heading(level, text));
            return this;
        }

        /// <summary>
        /// Creates a paragraph and appends it to the document.
        /// </summary>
        /// <param name="content">Paragraph text.</param>
        /// <returns>This builder for chaining.</returns>
        public DocumentBuilder AddParagraph(string content)
        {
            _document.AddElement(new Paragraph(content));
            return this;
        }

        /// <summary>
        /// Creates a link and appends it to the document.
        /// </summary>
        /// <param name="displayText">Visible link text.</param>
        /// <param name="url">Link target URL.</param>
        /// <returns>This builder for chaining.</returns>
        public DocumentBuilder AddLink(string displayText, string url)
        {
            _document.AddElement(new Link(displayText, url));
            return this;
        }

        /// <summary>
        /// Moves an already added element to a new position.
        /// </summary>
        /// <param name="currentIndex">Current position of the element.</param>
        /// <param name="newIndex">Target position of the element.</param>
        /// <returns>This builder for chaining.</returns>
        public DocumentBuilder MoveElement(int currentIndex, int newIndex)
        {
            _document.MoveElement(currentIndex, newIndex);
            return this;
        }

        /// <summary>
        /// Removes all added elements and starts over.
        /// </summary>
        /// <returns>This builder for chaining.</returns>
        public DocumentBuilder Clear()
        {
            _document.Clear();
            return this;
        }

        /// <summary>
        /// Renders the document built so far.
        /// </summary>
        /// <returns>Full rendered document text.</returns>
        public string RenderDocument() => _document.RenderDocument();

        /// <summary>
        /// Renders the table of contents of the document built so far.
        /// </summary>
        /// <returns>Formatted table of contents text.</returns>
        public string RenderTableOfContents() => _document.RenderTableOfContents();

        /// <summary>
        /// Returns the finished document under construction.
        /// </summary>
        /// <returns>The document built by this builder.</returns>
        public TextDocument Build() => _document;
    }
}
