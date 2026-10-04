using Xunit;
using Core.TextSystem;

namespace Tests.Core
{
    public sealed class TextDocumentTests
    {
        [Fact]
        public void AddMoveRemove_ReordersElements()
        {
            TextDocument document = new();
            Heading first = new(1, "A");
            Paragraph second = new("B");
            document.AddElement(first);
            document.AddElement(second);

            document.MoveElement(1, 0);

            Assert.Same(second, document.Elements[0]);
            Assert.True(document.RemoveElement(second));
            Assert.Equal(1, document.Count);
            document.Clear();
            Assert.Equal(0, document.Count);
        }

        [Fact]
        public void MoveElement_OutOfRange_IsIgnored()
        {
            TextDocument document = new();
            document.AddElement(new Paragraph("A"));

            document.MoveElement(-1, 5);
            document.MoveElement(5, 0);

            Assert.Equal(1, document.Count);
        }

        [Fact]
        public void RenderDocument_ConcatenatesRenders()
        {
            TextDocument document = new();
            document.AddElement(new Heading(1, "H"));
            document.AddElement(new Paragraph("P"));

            Assert.Equal("\n# H\nP\n", document.RenderDocument());
        }

        [Fact]
        public void TableOfContents_FlatDocument_MatchesLegacyFormat()
        {
            TextDocument document = new();
            document.AddElement(new Heading(1, "Top"));
            document.AddElement(new Paragraph("P"));
            document.AddElement(new Heading(2, "Sub"));

            string toc = document.RenderTableOfContents().Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("- Top\n", toc);
            Assert.Contains("  - Sub\n", toc);
            Assert.DoesNotContain("- P", toc);
        }

        [Fact]
        public void TableOfContents_SectionsRecurseWithDepth()
        {
            TextDocument document = new();
            Section outer = new("Outer");
            Section inner = new("Inner");
            inner.AddChild(new Heading(1, "Deep"));
            outer.AddChild(inner);
            document.AddElement(outer);

            string toc = document.RenderTableOfContents().Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("- Outer\n", toc);
            Assert.Contains("  - Inner\n", toc);
            Assert.Contains("    - Deep\n", toc);
        }
    }
}
