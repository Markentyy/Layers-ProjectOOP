namespace Infra.WebApi
{
    /// <summary>
    /// One character of the remote Genshin database.
    /// Holds lore only: base stats are generated locally by rarity.
    /// </summary>
    public sealed class GenshinCharacterDto
    {
        /// <summary>
        /// Gets or sets the character name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the character title.
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the vision (element).
        /// </summary>
        public string Vision { get; set; } = "";

        /// <summary>
        /// Gets or sets the weapon type.
        /// </summary>
        public string Weapon { get; set; } = "";

        /// <summary>
        /// Gets or sets the nation.
        /// </summary>
        public string Nation { get; set; } = "";

        /// <summary>
        /// Gets or sets the affiliation.
        /// </summary>
        public string Affiliation { get; set; } = "";

        /// <summary>
        /// Gets or sets the rarity (4 or 5 stars).
        /// </summary>
        public int Rarity { get; set; }

        /// <summary>
        /// Gets or sets the lore description.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the skill talents.
        /// </summary>
        public List<GenshinTalentDto> SkillTalents { get; set; } = new();
    }
}
