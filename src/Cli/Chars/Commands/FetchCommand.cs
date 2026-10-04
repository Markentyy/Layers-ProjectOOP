using Cli.Engine;
using Core.GameSystem;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// Imports a database weapon into the item registry.
    /// </summary>
    public sealed class FetchCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly IGenshinApiClient _api;
        private readonly CharsPresenter _presenter;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "fetch";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Fetch a database weapon as an item, then give it with add.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "fetch <weapon-db-id>";

        /// <summary>
        /// Initializes a fetch command.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="api">The database client.</param>
        /// <param name="presenter">The characters presenter.</param>
        public FetchCommand(CharsWorld world, IGenshinApiClient api, CharsPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(presenter);
            _world = world;
            _api = api;
            _presenter = presenter;
        }

        /// <summary>
        /// Fetches the database weapon and registers it.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string dbId = ResolveDbId(command.Args[0]);

            GenshinWeaponDto dto;
            try
            {
                dto = _api.GetWeaponAsync(dbId, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebApiException ex)
            {
                throw new CommandException(ex.Message);
            }

            Equipment item = GenshinMapper.ToEquipment(dto);
            string id = _world.AddItem(item, dbId);
            _presenter.ShowFetched(item.Name, id, item.AttackBonus, item.ArmorBonus);
        }

        /// <summary>
        /// Matches the input against database weapon ids, case insensitively.
        /// </summary>
        /// <param name="input">The user input.</param>
        /// <returns>The exact database id.</returns>
        private string ResolveDbId(string input)
        {
            IReadOnlyList<string> ids;
            try
            {
                ids = _api.GetWeaponIdsAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebApiException ex)
            {
                throw new CommandException(ex.Message);
            }
            string? match = ids.FirstOrDefault(id => id.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (match is null)
                throw new CommandException($"Unknown database weapon '{input}'. List them with: db weapons --search {input}");
            return match;
        }
    }
}
