using Xunit;
using Core.GameSystem;

namespace Tests.Core
{
    public sealed class CombatAndInventoryTests
    {
        [Fact]
        public void CalculateDamage_SubtractsArmor()
        {
            // Act & Assert
            Assert.Equal(12, CombatResolver.CalculateDamage(15, 3));
        }

        [Fact]
        public void CalculateDamage_FloorsAtZero()
        {
            // Act & Assert
            Assert.Equal(0, CombatResolver.CalculateDamage(3, 15));
        }

        [Fact]
        public void CalculateAbilityDamage_UsesMultiplier()
        {
            // Act & Assert
            Assert.Equal(30, CombatResolver.CalculateAbilityDamage(15, 2));
        }

        [Fact]
        public void CalculateAbilityDamage_DefaultsToConstant()
        {
            // Act & Assert
            Assert.Equal(
                15 * CombatResolver.DefaultAbilityMultiplier,
                CombatResolver.CalculateAbilityDamage(15));
        }

        [Fact]
        public void Inventory_AddRemoveAndTotals()
        {
            // Arrange
            var inventory = new Inventory();
            var sword = new Equipment("Sword", 10, 0);
            var shield = new Equipment("Shield", 0, 5);

            // Act
            inventory.Add(sword);
            inventory.Add(shield);

            // Assert
            Assert.Equal(2, inventory.Count);
            Assert.Equal(10, inventory.TotalAttackBonus);
            Assert.Equal(5, inventory.TotalArmorBonus);

            // Act & Assert
            Assert.True(inventory.Remove(sword));
            Assert.False(inventory.Remove(sword));
            Assert.Equal(0, inventory.TotalAttackBonus);
        }

        [Fact]
        public void Equipment_Validation()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Equipment("", 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Equipment("S", -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Equipment("S", 0, -1));
        }

        [Fact]
        public void Ability_Validation()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Ability("", 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ability("F", 0));
            Assert.Equal(CombatResolver.DefaultAbilityMultiplier, new Ability("F").DamageMultiplier);
        }
    }
}
