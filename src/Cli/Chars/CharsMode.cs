using Cli.Engine;

namespace Cli.Chars
{
    /// <summary>
    /// Builds the command set of the characters interpreter mode.
    /// </summary>
    public static class CharsMode
    {
        /// <summary>
        /// Gets the banner printed when the mode starts.
        /// </summary>
        public const string Intro =
            "Characters mode. Manage fighters, items and abilities. " +
            "Teach abilities with add before using them in act ability. " +
            "Type 'help' for commands.";

        /// <summary>
        /// Creates the characters command set bound to the given world.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <returns>The ready command set.</returns>
        public static CommandSet CreateCommands(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            CommandSet set = new();
            set.Add(new CreateCommand(world));
            set.Add(new AddCommand(world));
            set.Add(new ActCommand(world));
            set.Add(new LsCommand(world));
            set.Add(new HelpCommand(set));
            return set;
        }
    }
}
