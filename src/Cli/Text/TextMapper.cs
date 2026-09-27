using Cli.Engine;
using Core.TextSystem;
using Infra.Data;

namespace Cli.Text
{
    /// <summary>
    /// Converts between the live text document and its serializable snapshot.
    /// Lives in Cli (not Infra) so the Data layer never touches domain objects.
    /// </summary>
    public static class TextMapper
    {
        /// <summary>
        /// Captures the document into a snapshot.
        /// </summary>
        /// <param name="document">The document to capture.</param>
        /// <returns>The snapshot.</returns>
        public static TextSnapshot ToSnapshot(TextDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            TextSnapshot snapshot = new();
            foreach (TextElement element in document.Elements)
            {
                snapshot.Roots.Add(ToNode(element));
            }
            return snapshot;
        }

        /// <summary>
        /// Rebuilds a document from a snapshot, keeping stored ids.
        /// </summary>
        /// <param name="snapshot">The snapshot to restore.</param>
        /// <returns>The rebuilt document.</returns>
        public static TextDocument ToDocument(TextSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            TextDocument document = new();
            foreach (TextNode node in snapshot.Roots)
            {
                document.AddElement(ToElement(node));
            }
            return document;
        }

        /// <summary>
        /// Converts one element and its subtree into a node.
        /// </summary>
        /// <param name="element">The element to capture.</param>
        /// <returns>The node.</returns>
        private static TextNode ToNode(TextElement element)
        {
            TextNode node = new() { Id = element.Id };
            switch (element)
            {
                case Section section:
                    node.Kind = "section";
                    node.Title = section.Title;
                    foreach (TextElement child in section.Children)
                    {
                        node.Children.Add(ToNode(child));
                    }
                    break;
                case Heading heading:
                    node.Kind = "heading";
                    node.Text = heading.Text;
                    node.Level = heading.Level;
                    break;
                case Paragraph paragraph:
                    node.Kind = "paragraph";
                    node.Content = paragraph.Content;
                    break;
                case Link link:
                    node.Kind = "link";
                    node.DisplayText = link.DisplayText;
                    node.Url = link.Url;
                    break;
                default:
                    throw new CommandException($"Cannot save element of type '{element.GetType().Name}'.");
            }
            return node;
        }

        /// <summary>
        /// Rebuilds one element with its subtree from a node.
        /// </summary>
        /// <param name="node">The node to restore.</param>
        /// <returns>The rebuilt element.</returns>
        private static TextElement ToElement(TextNode node)
        {
            TextElement element = node.Kind switch
            {
                "section" => new Section(node.Title),
                "heading" => new Heading(node.Level <= 0 ? 1 : node.Level, node.Text),
                "paragraph" => new Paragraph(node.Content),
                "link" => new Link(node.DisplayText, node.Url),
                _ => throw new CommandException($"Invalid snapshot: unknown kind '{node.Kind}'."),
            };
            element.Id = node.Id;
            if (element is Section section)
            {
                foreach (TextNode child in node.Children)
                {
                    section.AddChild(ToElement(child));
                }
            }
            return element;
        }
    }
}
