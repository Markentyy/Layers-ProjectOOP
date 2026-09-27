using Cli.Engine;

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
        /// <returns>The ready command set.</returns>
        public static CommandSet CreateCommands(TextNavigator navigator)
        {
            ArgumentNullException.ThrowIfNull(navigator);
            CommandSet set = new();
            set.Add(new PwdCommand(navigator));
            set.Add(new PrintCommand(navigator));
            set.Add(new AddCommand(navigator));
            set.Add(new RmCommand(navigator));
            set.Add(new UpCommand(navigator));
            set.Add(new CdCommand(navigator));
            set.Add(new HelpCommand(set));
            return set;
        }
    }
}
