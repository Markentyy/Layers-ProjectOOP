namespace Infra.WebApi
{
    /// <summary>
    /// One skill talent of a database character.
    /// </summary>
    public sealed class GenshinTalentDto
    {
        /// <summary>
        /// Gets or sets the talent name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets how the talent unlocks (attack, skill, burst...).
        /// </summary>
        public string Unlock { get; set; } = "";

        /// <summary>
        /// Gets or sets the talent description.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Gets or sets the talent type (NORMAL_ATTACK, ELEMENTAL_SKILL, ELEMENTAL_BURST...).
        /// </summary>
        public string Type { get; set; } = "";
    }
}
