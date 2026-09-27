using Cli.Engine;
using Core.TextSystem;

using Infra.Display;
using Cli.Presenter;
namespace Cli.Text
{
    /// <summary>
    /// Creates a container or a leaf element in the current position via dialog.
    /// </summary>
    public sealed class AddCommand : IShellCommand
    {
        private readonly TextNavigator _navigator;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "add";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Create a container or leaf element here via dialog.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "add <container|leaf> <type>";

        /// <summary>
        /// Initializes an add command bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        public AddCommand(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            _navigator = navigator;
        }

        /// <summary>
        /// Runs the creation dialog for the requested element kind and type.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(2, 2, Usage);
            command.ExpectOptions(Usage);
            string kind = command.RequireArg(0, "a kind: add <container|leaf> <type>.");
            string type = command.RequireArg(1, "a type: add <container|leaf> <type>.");

            TextElement element = kind.ToLowerInvariant() switch
            {
                "container" => CreateContainer(console, type),
                "leaf" => CreateLeaf(console, type),
                _ => throw new CommandException($"Unknown kind '{kind}'. Use container or leaf."),
            };

            _navigator.AddToCurrent(element);
            console.WriteLine($"Created {element.GetType().Name} '{element.DisplayName}' [{element.Id}] in {_navigator.Pwd()}.");
        }

        /// <summary>
        /// Runs the container creation dialog. Only sections are supported.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="type">The requested container type.</param>
        /// <returns>The created container.</returns>
        private static TextElement CreateContainer(IDisplay console, string type)
        {
            if (!type.Equals("section", StringComparison.OrdinalIgnoreCase))
                throw new CommandException($"Unknown container type '{type}'. Use section.");
            string title = Prompter.ReadRequired(console, "Title");
            return new Section(title);
        }

        /// <summary>
        /// Runs the leaf creation dialog for headings, paragraphs and links.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="type">The requested leaf type.</param>
        /// <returns>The created leaf.</returns>
        private static TextElement CreateLeaf(IDisplay console, string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "heading":
                    int level = Prompter.ReadInt(console, "Level", 1, 6);
                    string text = Prompter.ReadRequired(console, "Text");
                    return new Heading(level, text);
                case "paragraph":
                    string content = Prompter.ReadRequired(console, "Content");
                    return new Paragraph(content);
                case "link":
                    string display = Prompter.ReadRequired(console, "DisplayText");
                    string url = Prompter.ReadRequired(console, "Url");
                    return new Link(display, url);
                default:
                    throw new CommandException($"Unknown leaf type '{type}'. Use heading, paragraph or link.");
            }
        }
    }
}
