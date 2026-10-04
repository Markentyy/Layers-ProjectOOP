using Xunit;
using Core.TextSystem;

namespace Tests.Core
{
    public sealed class TextElementTests
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(3, 3)]
        [InlineData(99, 6)]
        public void Heading_ClampsLevel(int level, int expected)
        {
            // Act & Assert
            Assert.Equal(expected, new Heading(level, "T").Level);
        }

        [Fact]
        public void Heading_RendersMarkdown()
        {
            // Act & Assert
            Assert.Equal("\n## Hi\n", new Heading(2, "Hi").Render());
        }

        [Fact]
        public void Paragraph_RendersWithNewline()
        {
            // Act & Assert
            Assert.Equal("Body\n", new Paragraph("Body").Render());
        }

        [Fact]
        public void Link_RendersMarkdown()
        {
            // Act & Assert
            Assert.Equal("[L](http://x.io)", new Link("L", "http://x.io").Render());
        }

        [Fact]
        public void DisplayNames_MatchLookupText()
        {
            // Act & Assert
            Assert.Equal("T", new Heading(1, "T").DisplayName);
            Assert.Equal("Body", new Paragraph("Body").DisplayName);
            Assert.Equal("L", new Link("L", "http://x.io").DisplayName);
            Assert.Equal("Sec", new Section("Sec").DisplayName);
            Assert.Equal("", new Paragraph("Body").Id);
        }

        [Fact]
        public void Section_RendersTitleAndChildren()
        {
            // Arrange
            Section section = new("Sec");
            section.AddChild(new Heading(1, "H"));
            section.AddChild(new Paragraph("P"));

            // Act
            string text = section.Render();

            // Assert
            Assert.Contains("# Sec", text);
            Assert.Contains("# H", text);
            Assert.Contains("P", text);
        }

        [Fact]
        public void Section_RemoveChild()
        {
            // Arrange
            Section section = new("Sec");
            Paragraph child = new("P");
            section.AddChild(child);

            // Act & Assert
            Assert.True(section.RemoveChild(child));
            Assert.False(section.RemoveChild(child));
            Assert.Equal(0, section.Count);
        }
    }
}
