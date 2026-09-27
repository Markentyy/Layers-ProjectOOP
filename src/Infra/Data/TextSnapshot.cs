namespace Infra.Data
{
    /// <summary>
    /// Serializable state of the whole text document.
    /// </summary>
    public sealed class TextSnapshot
    {
        /// <summary>
        /// Gets or sets the root elements.
        /// </summary>
        public List<TextNode> Roots { get; set; } = new();
    }
}
