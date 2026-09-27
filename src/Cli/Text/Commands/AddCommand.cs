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
        private readonly TextPresenter _presenter;

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
        /// Initializes an add command bound to the given navigator and presenter.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="presenter">The text presenter.</param>
        public AddCommand(TextNavigator navigator, TextPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(presenter);
            _navigator = navigator;
            _presenter = presenter;
        }

        /// <summary>
        /// Runs the creation dialog for the requested element kind and type.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(2, 2, Usage);
            command.ExpectOptions(Usage);
            string kind = command.RequireArg(0, "a kind: add <container|leaf> <type>.");
            string type = command.RequireArg(1, "a type: add <container|leaf> <type>.");

            TextElement element = kind.ToLowerInvariant() switch
            {
                "container" => CreateContainer(display, type),
                "leaf" => CreateLeaf(display, type),
                _ => throw new CommandException($"Unknown kind '{kind}'. Use container or leaf."),
            };

            _navigator.AddToCurrent(element);
            _presenter.ShowCreated(element.GetType().Name, element.DisplayName, element.Id, _navigator.Pwd());
        }

        /// <summary>
        /// Runs the container creation dialog. Only sections are supported.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        /// <param name="type">The requested container type.</param>
        /// <returns>The created container.</returns>
        private static TextElement CreateContainer(IDisplay display, string type)
        {
            if (!type.Equals("section", StringComparison.OrdinalIgnoreCase))
                throw new CommandException($"Unknown container type '{type}'. Use section.");
            string title = Prompter.ReadRequired(display, "Title");
            return new Section(title);
        }

        /// <summary>
        /// Runs the leaf creation dialog for headings, paragraphs and links.
        /// </summary>
        /// <param name="display">The display for dialog input.</param>
        /// <param name="type">The requested leaf type.</param>
        /// <returns>The created leaf.</returns>
        private static TextElement CreateLeaf(IDisplay display, string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "heading":
                    int level = Prompter.ReadInt(display, "Level", 1, 6);
                    string text = Prompter.ReadRequired(display, "Text");
                    return new Heading(level, text);
                case "paragraph":
                    string content = Prompter.ReadRequired(display, "Content");
                    return new Paragraph(content);
                case "link":
                    string linkText = Prompter.ReadRequired(display, "DisplayText");
                    string url = Prompter.ReadRequired(display, "Url");
                    return new Link(linkText, url);
                default:
                    throw new CommandException($"Unknown leaf type '{type}'. Use heading, paragraph or link.");
            }
        }
    }
}
