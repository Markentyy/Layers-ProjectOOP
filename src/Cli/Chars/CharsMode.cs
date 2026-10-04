using Cli.Engine;
using Cli.Presenter;
using Infra.Data;
using Infra.Display;
using Infra.WebApi;

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
        /// <param name="display">The display for presenters.</param>
        /// <param name="store">The characters store for save and load.</param>
        /// <param name="api">The remote database client.</param>
        /// <param name="random">The dice for stat rolls.</param>
        /// <returns>The ready command set.</returns>
        public static CommandSet CreateCommands(
            CharsWorld world, IDisplay display, ICharsStore store, IGenshinApiClient api, Random random)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(display);
            ArgumentNullException.ThrowIfNull(store);
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(random);
            CharsPresenter presenter = new(display);
            CommandSet set = new();
            set.Add(new CreateCommand(world, presenter));
            set.Add(new AddCommand(world, presenter));
            set.Add(new ActCommand(world, presenter));
            set.Add(new LsCommand(world, presenter));
            set.Add(new SaveCommand(world, store, presenter));
            set.Add(new LoadCommand(world, store, presenter));
            set.Add(new DbCommand(api, presenter));
            set.Add(new RecruitCommand(world, api, presenter, random));
            set.Add(new FetchCommand(world, api, presenter));
            set.Add(new ShowCommand(world, presenter));
            set.Add(new HelpCommand(set));
            return set;
        }
    }
}
