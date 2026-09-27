namespace Core.TextSystem
{
    /// <summary>
    /// Represents a foundational text element within a document.
    /// </summary>
    public interface ITextElement
    {
        /// <summary>
        /// Renders the specific element to its string representation.
        /// </summary>
        /// <returns>Formatted string content.</returns>
        string Render();
    }
}
