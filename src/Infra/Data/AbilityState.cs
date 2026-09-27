namespace Infra.Data
{
    /// <summary>
    /// Serializable state of one ability.
    /// </summary>
    public sealed class AbilityState
    {
        /// <summary>
        /// Gets or sets the ability id.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Gets or sets the ability name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the damage multiplier.
        /// </summary>
        public int DamageMultiplier { get; set; }
    }
}
