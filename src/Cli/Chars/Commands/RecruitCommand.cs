using Cli.Engine;
using Core.GameSystem;
using Infra.Data;
using Infra.Display;
using Cli.Presenter;

using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// Creates a virtual hero from a database entry with rolled stats.
    /// </summary>
    public sealed class RecruitCommand : IShellCommand
    {
        private readonly CharsWorld _world;
        private readonly IGenshinApiClient _api;
        private readonly CharsPresenter _presenter;
        private readonly Random _random;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "recruit";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Recruit a database hero with rolled stats and learned talents.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "recruit <db-id>";

        /// <summary>
        /// Initializes a recruit command.
        /// </summary>
        /// <param name="world">The characters world.</param>
        /// <param name="api">The database client.</param>
        /// <param name="presenter">The characters presenter.</param>
        /// <param name="random">The dice for stat rolls.</param>
        public RecruitCommand(CharsWorld world, IGenshinApiClient api, CharsPresenter presenter, Random random)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(api);
            ArgumentNullException.ThrowIfNull(presenter);
            ArgumentNullException.ThrowIfNull(random);
            _world = world;
            _api = api;
            _presenter = presenter;
            _random = random;
        }

        /// <summary>
        /// Fetches the database hero and registers it with talents.
        /// </summary>
        /// <param name="display">The display (unused, kept for the contract).</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay display, ParsedCommand command)
        {
            command.ExpectArgCount(1, 1, Usage);
            command.ExpectOptions(Usage);
            string dbId = ResolveDbId(command.Args[0]);

            GenshinCharacterDto lore;
            try
            {
                lore = _api.GetCharacterAsync(dbId, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebApiException ex)
            {
                throw new CommandException(ex.Message);
            }

            (int maxHealth, int armor, int attack) = GenshinMapper.RollStats(lore.Rarity, _random);
            Character character = GenshinMapper.ToCharacter(lore, maxHealth, armor, attack);
            string id;
            try
            {
                id = _world.AddCharacter(character, dbId);
            }
            catch (CommandException)
            {
                throw new CommandException($"Id '{dbId}' is already recruited. Pick another hero.");
            }
            _world.Lore[id] = lore;

            int learned = 0;
            for (int i = 0; i < lore.SkillTalents.Count; i++)
            {
                Ability ability = GenshinMapper.ToAbility(lore.SkillTalents[i]);
                string abilityId = $"{dbId}-t{i + 1}";
                _world.AddAbility(ability, abilityId);
                _world.Learn(id, abilityId);
                learned++;
            }

            _presenter.ShowRecruited(character.Name, id, maxHealth, armor, attack, learned);
        }

        /// <summary>
        /// Matches the input against database ids, case insensitively.
        /// </summary>
        /// <param name="input">The user input.</param>
        /// <returns>The exact database id.</returns>
        private string ResolveDbId(string input)
        {
            IReadOnlyList<string> ids;
            try
            {
                ids = _api.GetCharacterIdsAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (WebApiException ex)
            {
                throw new CommandException(ex.Message);
            }
            string? match = ids.FirstOrDefault(id => id.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (match is null)
                throw new CommandException($"Unknown database hero '{input}'. List them with: db chars --search {input}");
            return match;
        }
    }
}
