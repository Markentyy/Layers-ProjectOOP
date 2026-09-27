namespace Core.TextSystem
{
    /// <summary>
    /// Represents a hyperlink element.
    /// </summary>
    public sealed class Link : TextElement
    {
        /// <summary>
        /// Gets the visible link text.
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// Gets the link target URL.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Initializes a new link with display text and URL.
        /// </summary>
        /// <param name="displayText">Visible link text.</param>
        /// <param name="url">Link target URL.</param>
        public Link(string displayText, string url)
        {
            ArgumentNullException.ThrowIfNull(displayText);
            ArgumentNullException.ThrowIfNull(url);
            DisplayText = displayText;
            Url = url;
        }

        /// <summary>
        /// Gets the visible link text for lookup by name.
        /// </summary>
        /// <returns>The visible link text.</returns>
        public override string DisplayName => DisplayText;

        /// <summary>
        /// Renders the link in markdown style.
        /// </summary>
        /// <returns>Formatted link string.</returns>
        public override string Render() => $"[{DisplayText}]({Url})";
    }
}
