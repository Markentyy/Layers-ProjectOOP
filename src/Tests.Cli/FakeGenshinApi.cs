using Xunit;
using Infra.WebApi;

namespace Tests.Cli
{
    /// <summary>
    /// In-memory database double with canned heroes and weapons.
    /// Replaces HTTP calls without extra packages.
    /// </summary>
    public sealed class FakeGenshinApi : IGenshinApiClient
    {
        /// <summary>
        /// Gets the canned character ids.
        /// </summary>
        public List<string> CharIds { get; } = new() { "albedo", "klee" };

        /// <summary>
        /// Gets the canned characters by id.
        /// </summary>
        public Dictionary<string, GenshinCharacterDto> Characters { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the canned weapon ids.
        /// </summary>
        public List<string> WeaponIds { get; } = new() { "skyward-blade" };

        /// <summary>
        /// Gets the canned weapons by id.
        /// </summary>
        public Dictionary<string, GenshinWeaponDto> Weapons { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a fake with one hero and one weapon.
        /// </summary>
        public FakeGenshinApi()
        {
            Characters["albedo"] = new GenshinCharacterDto
            {
                Name = "Albedo",
                Title = "Kreideprinz",
                Vision = "Geo",
                Weapon = "Sword",
                Nation = "Mondstadt",
                Affiliation = "Knights of Favonius",
                Rarity = 5,
                Description = "A genius alchemist of Mondstadt.",
                SkillTalents =
                {
                    new GenshinTalentDto { Name = "Weiss", Unlock = "Normal Attack", Description = "Rapid strikes.", Type = "NORMAL_ATTACK" },
                    new GenshinTalentDto { Name = "Isotoma", Unlock = "Elemental Skill", Description = "Solar flower.", Type = "ELEMENTAL_SKILL" },
                    new GenshinTalentDto { Name = "Tectonic Tide", Unlock = "Elemental Burst", Description = "Big boom.", Type = "ELEMENTAL_BURST" },
                },
            };
            Characters["klee"] = new GenshinCharacterDto
            {
                Name = "Klee",
                Title = "Fleeing Sunlight",
                Vision = "Pyro",
                Weapon = "Catalyst",
                Nation = "Mondstadt",
                Affiliation = "Knights of Favonius",
                Rarity = 5,
                Description = "A small spark of joy.",
            };
            Weapons["skyward-blade"] = new GenshinWeaponDto
            {
                Id = "skyward-blade",
                Name = "Skyward Blade",
                Type = "Sword",
                Rarity = 5,
                BaseAttack = 44,
                SubStat = "Energy Recharge",
                PassiveName = "Sky-Piercing Fang",
            };
        }

        /// <summary>
        /// Lists the canned character ids.
        /// </summary>
        /// <param name="token">The cancellation token (ignored).</param>
        /// <returns>The character ids.</returns>
        public Task<IReadOnlyList<string>> GetCharacterIdsAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<string>>(CharIds);

        /// <summary>
        /// Reads one canned character or fails like a 404.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="token">The cancellation token (ignored).</param>
        /// <returns>The character data.</returns>
        public Task<GenshinCharacterDto> GetCharacterAsync(string id, CancellationToken token) =>
            Characters.TryGetValue(id, out GenshinCharacterDto? dto)
                ? Task.FromResult(dto)
                : Task.FromException<GenshinCharacterDto>(new WebApiException($"Unknown character '{id}'."));

        /// <summary>
        /// Lists the canned weapon ids.
        /// </summary>
        /// <param name="token">The cancellation token (ignored).</param>
        /// <returns>The weapon ids.</returns>
        public Task<IReadOnlyList<string>> GetWeaponIdsAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<string>>(WeaponIds);

        /// <summary>
        /// Reads one canned weapon or fails like a 404.
        /// </summary>
        /// <param name="id">The weapon id.</param>
        /// <param name="token">The cancellation token (ignored).</param>
        /// <returns>The weapon data.</returns>
        public Task<GenshinWeaponDto> GetWeaponAsync(string id, CancellationToken token) =>
            Weapons.TryGetValue(id, out GenshinWeaponDto? dto)
                ? Task.FromResult(dto)
                : Task.FromException<GenshinWeaponDto>(new WebApiException($"Unknown weapon '{id}'."));
    }
}
