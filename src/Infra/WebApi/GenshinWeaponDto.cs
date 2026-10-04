namespace Infra.WebApi
{
    /// <summary>
    /// One weapon of the remote Genshin database.
    /// Unlike characters, weapons carry base stats.
    /// </summary>
    public sealed class GenshinWeaponDto
    {
        /// <summary>
        /// Gets or sets the weapon id used by the API.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Gets or sets the weapon name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the weapon type.
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Gets or sets the rarity (stars).
        /// </summary>
        public int Rarity { get; set; }

        /// <summary>
        /// Gets or sets the base attack.
        /// </summary>
        public int BaseAttack { get; set; }

        /// <summary>
        /// Gets or sets the sub stat name.
        /// </summary>
        public string SubStat { get; set; } = "";

        /// <summary>
        /// Gets or sets the passive ability name.
        /// </summary>
        public string PassiveName { get; set; } = "";

        /// <summary>
        /// Gets or sets the passive ability description.
        /// </summary>
        public string PassiveDesc { get; set; } = "";
    }
}
