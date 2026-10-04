using Cli.Engine;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// Lists database characters and weapons with paging and search.
    /// </summary>
    public sealed class DbCommand : IShellCommand
    {
        private readonly IGenshinApiClient _api;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "db";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "List database characters or weapons.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "db <chars|weapons> [--search <id-text>] [--limit <n>]";

        /// <summary>
        /// Initializes a db command.
        /// </summary>
        /// <param name="api">The database client.</param>
        /// <param name="presenter">The characters presenter.</param>
        public DbCommand(IGenshinApiClient api, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(presenter);
            _api = api;
            _presenter = presenter;
        }

        /// <summary>
        /// Lists one database page.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage, "search", "limit");
            string kind = command.RequireArg(0, "a category: db <chars|weapons>.");
            string? search = command.GetOption("search");
            int limit = ReadLimit(command);

            switch (kind.ToLowerInvariant())
            {
                case "chars":
                    ListCharacters(search, limit);
                    break;
                case "weapons":
                    ListWeapons(search, limit);
                    break;
                default:
                    throw new CommandException($"Unknown category '{kind}'. Use chars or weapons.");
            }
        }

        /// <summary>
        /// Reads and validates the limit option (20 by default, 0 means all).
        /// </summary>
        /// <param name="command">The parsed command line.</param>
        /// <returns>The row limit.</returns>
        private static int ReadLimit(ParsedCommand command)
        {
            string? raw = command.GetOption("limit");
            if (raw is null)
                return 20;
            if (!int.TryParse(raw, out int limit) || limit < 0)
                throw new CommandException("Option --limit must be a number >= 0.");
            return limit;
        }

        /// <summary>
        /// Prints one page of database characters.
        /// </summary>
        /// <param name="search">The id substring filter, or null.</param>
        /// <param name="limit">The row limit, 0 means all.</param>
        private void ListCharacters(string? search, int limit)
        {
            IReadOnlyList<string> ids = Run(_api.GetCharacterIdsAsync(CancellationToken.None));
            int shown = 0;
            foreach (string id in ids)
            {
                if (limit != 0 && shown >= limit)
                    break;
                if (search is not null && !id.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;
                GenshinCharacterDto character = Run(_api.GetCharacterAsync(id, CancellationToken.None));
                _presenter.ShowDbCharacter(id, character);
                shown++;
            }
        }

        /// <summary>
        /// Prints one page of database weapons.
        /// </summary>
        /// <param name="search">The id substring filter, or null.</param>
        /// <param name="limit">The row limit, 0 means all.</param>
        private void ListWeapons(string? search, int limit)
        {
            IReadOnlyList<string> ids = Run(_api.GetWeaponIdsAsync(CancellationToken.None));
            int shown = 0;
            foreach (string id in ids)
            {
                if (limit != 0 && shown >= limit)
                    break;
                if (search is not null && !id.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;
                GenshinWeaponDto weapon = Run(_api.GetWeaponAsync(id, CancellationToken.None));
                _presenter.ShowDbWeapon(id, weapon);
                shown++;
            }
        }

        /// <summary>
        /// Runs an API call synchronously, translating transport errors.
        /// Blocking is safe here: a console loop owns no synchronization context.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="task">The API task.</param>
        /// <returns>The API result.</returns>
        private static T Run<T>(Task<T> task)
        {
            try
            {
                return task.GetAwaiter().GetResult();
            }
            catch (WebApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CommandException($"Database call failed: {ex.Message}");
            }
        }
    }
}
