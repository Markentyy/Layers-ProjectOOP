namespace Infra.Data
{
    /// <summary>
    /// Serializable state of one character, including owned item and ability ids.
    /// </summary>
    public sealed class CharacterState
    {
        /// <summary>
        /// Gets or sets the character id.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Gets or sets the character name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the maximum health.
        /// </summary>
        public int MaxHealth { get; set; }

        /// <summary>
        /// Gets or sets the current health.
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Gets or sets the base armor.
        /// </summary>
        public int BaseArmor { get; set; }

        /// <summary>
        /// Gets or sets the base attack.
        /// </summary>
        public int BaseAttack { get; set; }

        /// <summary>
        /// Gets or sets whether the defensive stance was active.
        /// </summary>
        public bool IsDefending { get; set; }

        /// <summary>
        /// Gets or sets the equipped item ids.
        /// </summary>
        public List<string> ItemIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the learned ability ids.
        /// </summary>
        public List<string> AbilityIds { get; set; } = new();
    }
}
