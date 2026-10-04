using Xunit;
using Cli.Chars;
using Cli.Engine;
using Cli.Presenter;
using Core.GameSystem;
using Infra.Data;
using Infra.WebApi;

namespace Tests.Cli
{
    public sealed class GenshinMapperTests
    {
        [Theory]
        [InlineData(5, 90, 120, 3, 6, 20, 30)]
        [InlineData(4, 60, 90, 1, 4, 12, 20)]
        [InlineData(3, 40, 70, 0, 2, 8, 14)]
        [InlineData(99, 40, 70, 0, 2, 8, 14)]
        public void RollStats_StaysInRarityRange(
            int rarity, int minHp, int maxHp, int minArm, int maxArm, int minAtk, int maxAtk)
        {
            // Arrange
            Random random = new(42);

            // Act
            (int hp, int armor, int attack) = GenshinMapper.RollStats(rarity, random);

            // Assert
            Assert.InRange(hp, minHp, maxHp);
            Assert.InRange(armor, minArm, maxArm);
            Assert.InRange(attack, minAtk, maxAtk);
        }

        [Theory]
        [InlineData("ELEMENTAL_BURST", 3)]
        [InlineData("elemental_skill", 2)]
        [InlineData("NORMAL_ATTACK", 1)]
        [InlineData("whatever", 1)]
        public void MultiplierFor_MapsTalentType(string type, int expected)
        {
            // Act & Assert
            Assert.Equal(expected, GenshinMapper.MultiplierFor(type));
        }

        [Fact]
        public void ToCharacter_UsesNameAndStats()
        {
            // Arrange
            GenshinCharacterDto lore = new() { Name = "Albedo", Rarity = 5 };

            // Act
            Character character = GenshinMapper.ToCharacter(lore, 100, 5, 25);

            // Assert
            Assert.Equal("Albedo", character.Name);
            Assert.Equal(100, character.MaxHealth);
        }

        [Fact]
        public void ToAbility_UsesTalentNameAndMultiplier()
        {
            // Arrange
            GenshinTalentDto talent = new() { Name = "Boom", Type = "ELEMENTAL_BURST" };

            // Act
            Ability ability = GenshinMapper.ToAbility(talent);

            // Assert
            Assert.Equal("Boom", ability.Name);
            Assert.Equal(3, ability.DamageMultiplier);
        }

        [Fact]
        public void ToEquipment_ScalesBaseAttackAndRarity()
        {
            // Arrange
            GenshinWeaponDto weapon = new() { Name = "Blade", BaseAttack = 44, Rarity = 5 };

            // Act
            Equipment item = GenshinMapper.ToEquipment(weapon);

            // Assert
            Assert.Equal("Blade", item.Name);
            Assert.Equal(4, item.AttackBonus);
            Assert.Equal(5, item.ArmorBonus);
        }

        [Fact]
        public void ToEquipment_NegativeStats_ClampToZero()
        {
            // Arrange
            GenshinWeaponDto weapon = new() { Name = "Stick", BaseAttack = -5, Rarity = -1 };

            // Act
            Equipment item = GenshinMapper.ToEquipment(weapon);

            // Assert
            Assert.Equal(0, item.AttackBonus);
            Assert.Equal(0, item.ArmorBonus);
        }

        [Fact]
        public void CharsWorldMapper_PersistsLore()
        {
            // Arrange
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 10, 0, 1), "a");
            world.Lore["a"] = new GenshinCharacterDto { Name = "Albedo", Rarity = 5 };
            CharsWorld restored = new();

            // Act
            CharsSnapshot snapshot = CharsWorldMapper.ToSnapshot(world);
            CharsWorldMapper.LoadInto(restored, snapshot);

            // Assert
            Assert.Equal("Albedo", restored.Lore["a"].Name);
        }
    }
}
