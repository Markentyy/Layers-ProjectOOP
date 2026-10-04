using Cli.Engine;
using Core.GameSystem;
using Infra.Data;

using Infra.WebApi;
namespace Cli.Chars
{
    /// <summary>
    /// Translates remote database entries into domain objects.
    /// The database holds lore only, so combat stats are generated
    /// randomly by rarity within documented ranges.
    /// </summary>
    public static class GenshinMapper
    {
        /// <summary>
        /// Rolls combat stats for the given rarity.
        /// 5 stars: HP 90-120, ATK 20-30, ARM 3-6.
        /// 4 stars: HP 60-90, ATK 12-20, ARM 1-4.
        /// Other: HP 40-70, ATK 8-14, ARM 0-2.
        /// </summary>
        /// <param name="rarity">The star rarity.</param>
        /// <param name="random">The dice.</param>
        /// <returns>The rolled max health, armor and attack.</returns>
        public static (int maxHealth, int armor, int attack) RollStats(int rarity, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);
            return rarity switch
            {
                5 => (random.Next(90, 121), random.Next(3, 7), random.Next(20, 31)),
                4 => (random.Next(60, 91), random.Next(1, 5), random.Next(12, 21)),
                _ => (random.Next(40, 71), random.Next(0, 3), random.Next(8, 15)),
            };
        }

        /// <summary>
        /// Builds a character from database lore and explicit stats.
        /// </summary>
        /// <param name="lore">The database character.</param>
        /// <param name="maxHealth">The maximum health.</param>
        /// <param name="baseArmor">The base armor.</param>
        /// <param name="baseAttack">The base attack.</param>
        /// <returns>The domain character.</returns>
        public static Character ToCharacter(GenshinCharacterDto lore, int maxHealth, int baseArmor, int baseAttack)
        {
            ArgumentNullException.ThrowIfNull(lore);
            return new Character(lore.Name, maxHealth, baseArmor, baseAttack);
        }

        /// <summary>
        /// Picks the damage multiplier by talent type.
        /// Burst counts 3, skill counts 2, everything else counts 1.
        /// </summary>
        /// <param name="type">The talent type string.</param>
        /// <returns>The multiplier.</returns>
        public static int MultiplierFor(string type) =>
            type.ToUpperInvariant() switch
            {
                "ELEMENTAL_BURST" => 3,
                "ELEMENTAL_SKILL" => 2,
                _ => 1,
            };

        /// <summary>
        /// Builds an ability from one database talent.
        /// </summary>
        /// <param name="talent">The database talent.</param>
        /// <returns>The domain ability.</returns>
        public static Ability ToAbility(GenshinTalentDto talent)
        {
            ArgumentNullException.ThrowIfNull(talent);
            return new Ability(talent.Name, MultiplierFor(talent.Type));
        }

        /// <summary>
        /// Builds an item from one database weapon.
        /// Attack bonus is base attack over ten, armor bonus is the rarity.
        /// </summary>
        /// <param name="weapon">The database weapon.</param>
        /// <returns>The domain equipment.</returns>
        public static Equipment ToEquipment(GenshinWeaponDto weapon)
        {
            ArgumentNullException.ThrowIfNull(weapon);
            return new Equipment(weapon.Name, Math.Max(0, weapon.BaseAttack / 10), Math.Max(0, weapon.Rarity));
        }
    }
}
