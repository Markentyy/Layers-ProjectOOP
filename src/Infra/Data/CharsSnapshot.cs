namespace Infra.Data
{
    /// <summary>
    /// Serializable state of the whole characters world.
    /// </summary>
    public sealed class CharsSnapshot
    {
        /// <summary>
        /// Gets or sets the stored characters.
        /// </summary>
        public List<CharacterState> Characters { get; set; } = new();

        /// <summary>
        /// Gets or sets the stored items.
        /// </summary>
        public List<ItemState> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the stored abilities.
        /// </summary>
        public List<AbilityState> Abilities { get; set; } = new();
    }
}
