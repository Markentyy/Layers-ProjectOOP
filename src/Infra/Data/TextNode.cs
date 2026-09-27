namespace Infra.Data
{
    /// <summary>
    /// Serializable state of one document element with its subtree.
    /// Kind is one of: section, heading, paragraph, link.
    /// </summary>
    public sealed class TextNode
    {
        /// <summary>
        /// Gets or sets the element id.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Gets or sets the element kind.
        /// </summary>
        public string Kind { get; set; } = "";

        /// <summary>
        /// Gets or sets the section title.
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the heading text.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Gets or sets the heading level.
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets the paragraph content.
        /// </summary>
        public string Content { get; set; } = "";

        /// <summary>
        /// Gets or sets the link display text.
        /// </summary>
        public string DisplayText { get; set; } = "";

        /// <summary>
        /// Gets or sets the link URL.
        /// </summary>
        public string Url { get; set; } = "";

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        public List<TextNode> Children { get; set; } = new();
    }
}
