using Cli.Engine;
using Cli.Presenter;
using Infra.Data;
using Infra.Display;

namespace Cli.Text
{
    /// <summary>
    /// Builds the command set of the text interpreter mode.
    /// </summary>
    public static class TextMode
    {
        /// <summary>
        /// Gets the banner printed when the mode starts.
        /// </summary>
        public const string Intro =
            "Text mode. Navigate and edit the structured document. Type 'help' for commands.";

        /// <summary>
        /// Creates the text command set bound to the given navigator.
        /// </summary>
        /// <param name="navigator">The text navigator.</param>
        /// <param name="display">The display for presenters.</param>
        /// <param name="store">The text store for save and load.</param>
        /// <returns>The ready command set.</returns>
        public static CommandSet CreateCommands(TextNavigator navigator, IDisplay display, ITextStore store)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            ArgumentNullException.ThrowIfNull(display);
            ArgumentNullException.ThrowIfNull(store);
            TextPresenter presenter = new(display);
            CommandSet set = new();
            set.Add(new PwdCommand(navigator, presenter));
            set.Add(new PrintCommand(navigator, presenter));
            set.Add(new AddCommand(navigator, presenter));
            set.Add(new RmCommand(navigator, presenter));
            set.Add(new UpCommand(navigator, presenter));
            set.Add(new CdCommand(navigator, presenter));
            set.Add(new SaveCommand(navigator, store, presenter));
            set.Add(new LoadCommand(navigator, store, presenter));
            set.Add(new HelpCommand(set));
            return set;
        }
    }
}
