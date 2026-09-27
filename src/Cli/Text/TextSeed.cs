using Core.TextSystem;

namespace Cli.Text
{
    /// <summary>
    /// Builds the starter document used by the text interpreter mode.
    /// </summary>
    public static class TextSeed
    {
        /// <summary>
        /// Creates a small nested document about OOP.
        /// </summary>
        /// <returns>The seeded document.</returns>
        public static TextDocument CreateDemoDocument()
        {
            TextDocument document = new();

            Section oop = new("oop");
            oop.AddChild(new Heading(1, "Object-Oriented Programming"));
            oop.AddChild(new Paragraph("OOP is a programming paradigm based on the concept of objects."));

            Section pillars = new("pillars");
            pillars.AddChild(new Heading(2, "Core Principles"));
            pillars.AddChild(new Paragraph("The four pillars are Encapsulation, Abstraction, Inheritance, and Polymorphism."));
            pillars.AddChild(new Link("Read more here", "https://docs.microsoft.com/dotnet/csharp/fundamentals/tutorials/oop"));

            oop.AddChild(pillars);
            document.AddElement(oop);
            return document;
        }
    }
}
