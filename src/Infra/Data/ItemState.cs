namespace Infra.Data
{
    /// <summary>
    /// Serializable state of one item.
    /// </summary>
    public sealed class ItemState
    {
        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Gets or sets the item name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the attack bonus.
        /// </summary>
        public int AttackBonus { get; set; }

        /// <summary>
        /// Gets or sets the armor bonus.
        /// </summary>
        public int ArmorBonus { get; set; }
    }
}
