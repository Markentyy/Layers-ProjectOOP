using Cli.Engine;
using Core.GameSystem;
using Infra.Data;

namespace Cli.Chars
{
    /// <summary>
    /// Converts between the live characters world and its serializable snapshot.
    /// Lives in Cli (not Infra) so the Data layer never touches domain objects.
    /// </summary>
    public static class CharsWorldMapper
    {
        /// <summary>
        /// Captures the world state into a snapshot.
        /// </summary>
        /// <param name="world">The world to capture.</param>
        /// <returns>The snapshot.</returns>
        public static CharsSnapshot ToSnapshot(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            CharsSnapshot snapshot = new();

            foreach (KeyValuePair<string, Equipment> entry in world.Items.All)
            {
                snapshot.Items.Add(new ItemState
                {
                    Id = entry.Key,
                    Name = entry.Value.Name,
                    AttackBonus = entry.Value.AttackBonus,
                    ArmorBonus = entry.Value.ArmorBonus,
                });
            }

            foreach (KeyValuePair<string, Ability> entry in world.Abilities.All)
            {
                snapshot.Abilities.Add(new AbilityState
                {
                    Id = entry.Key,
                    Name = entry.Value.Name,
                    DamageMultiplier = entry.Value.DamageMultiplier,
                });
            }

            foreach (KeyValuePair<string, Character> entry in world.Characters.All)
            {
                Character c = entry.Value;
                List<string> itemIds = new();
                foreach (Equipment item in c.Inventory.Items)
                {
                    if (world.Items.TryGetId(item, out string? itemId) && itemId is not null)
                        itemIds.Add(itemId);
                }
                snapshot.Characters.Add(new CharacterState
                {
                    Id = entry.Key,
                    Name = c.Name,
                    MaxHealth = c.MaxHealth,
                    Health = c.Health,
                    BaseArmor = c.BaseArmor,
                    BaseAttack = c.BaseAttack,
                    IsDefending = c.IsDefending,
                    ItemIds = itemIds,
                    AbilityIds = new List<string>(world.BookOf(entry.Key)),
                });
            }

            return snapshot;
        }

        /// <summary>
        /// Replaces the world content with the snapshot state.
        /// </summary>
        /// <param name="world">The world to refill.</param>
        /// <param name="snapshot">The snapshot to restore.</param>
        public static void LoadInto(CharsWorld world, CharsSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(world);
            ArgumentNullException.ThrowIfNull(snapshot);
            try
            {
                world.Clear();

                foreach (ItemState state in snapshot.Items)
                {
                    world.Items.Add(new Equipment(state.Name, state.AttackBonus, state.ArmorBonus), state.Id);
                }

                foreach (AbilityState state in snapshot.Abilities)
                {
                    world.Abilities.Add(new Ability(state.Name, state.DamageMultiplier), state.Id);
                }

                foreach (CharacterState state in snapshot.Characters)
                {
                    var character = new Character(state.Name, state.MaxHealth, state.BaseArmor, state.BaseAttack);
                    string id = world.Characters.Add(character, state.Id);
                    character.RestoreState(state.Health, state.IsDefending);
                    foreach (string itemId in state.ItemIds)
                    {
                        character.Equip(world.Items.GetById(itemId));
                    }
                    foreach (string abilityId in state.AbilityIds)
                    {
                        if (!world.Abilities.ContainsId(abilityId))
                            throw new ArgumentException($"Unknown ability id '{abilityId}'.");
                        world.Learn(id, abilityId);
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                throw new CommandException($"Invalid snapshot: {ex.Message}");
            }
        }
    }
}
