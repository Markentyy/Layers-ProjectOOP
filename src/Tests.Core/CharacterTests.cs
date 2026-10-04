using Xunit;
using Core.GameSystem;

namespace Tests.Core
{
    public sealed class CharacterTests
    {
        private static Character Warrior() => new("Arthur", 100, 5, 15);
        private static Character Mage() => new("Merlin", 70, 2, 25);

        [Fact]
        public void Attack_DealsAttackMinusArmor()
        {
            Character warrior = Warrior();
            Character mage = Mage();

            int damage = mage.Attack(warrior);

            Assert.Equal(20, damage);
            Assert.Equal(80, warrior.Health);
        }

        [Fact]
        public void Attack_NeverDealsNegativeDamage()
        {
            Character tank = new("Tank", 100, 1000, 0);
            Character weak = new("Weak", 100, 0, 1);

            int damage = weak.Attack(tank);

            Assert.Equal(0, damage);
            Assert.Equal(100, tank.Health);
        }

        [Fact]
        public void Attack_LeavesDefendingStance()
        {
            Character warrior = Warrior();
            warrior.Defend();
            Assert.True(warrior.IsDefending);

            warrior.Attack(Mage());

            Assert.False(warrior.IsDefending);
        }

        [Fact]
        public void Defend_AddsArmorBonus()
        {
            Character warrior = Warrior();
            int plain = warrior.TotalArmor;

            warrior.Defend();

            Assert.Equal(plain + CombatResolver.DefendArmorBonus, warrior.TotalArmor);
        }

        [Fact]
        public void Heal_RestoresButClampsToMax()
        {
            Character warrior = Warrior();
            Mage().Attack(warrior);
            Assert.Equal(80, warrior.Health);

            warrior.Heal(30);

            Assert.Equal(100, warrior.Health);
        }

        [Fact]
        public void Heal_NegativeAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Warrior().Heal(-1));
        }

        [Fact]
        public void Health_ClampsAtZero_AndReportsDefeat()
        {
            Character mage = Mage();

            for (int i = 0; i < 4; i++)
                mage.Attack(mage);

            Assert.Equal(0, mage.Health);
            Assert.True(mage.IsDefeated);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_EmptyName_Throws(string name)
        {
            Assert.Throws<ArgumentException>(() => new Character(name, 100, 5, 15));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Constructor_BadMaxHealth_Throws(int maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Character("A", maxHealth, 5, 15));
        }

        [Fact]
        public void Constructor_NegativeStats_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Character("A", 100, -1, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Character("A", 100, 5, -1));
        }

        [Fact]
        public void Equip_RaisesTotalsThroughInventory()
        {
            Character warrior = Warrior();

            warrior.Equip(new Equipment("Shield", 0, 10));

            Assert.Equal(15, warrior.TotalAttack);
            Assert.Equal(15, warrior.TotalArmor);
            Assert.Single(warrior.Inventory.Items);
        }

        [Fact]
        public void UseAbility_DealsAttackTimesMultiplier()
        {
            Character mage = Mage();
            Character warrior = Warrior();

            int damage = mage.UseAbility(new Ability("Fireball", 2), warrior);

            Assert.Equal(50, damage);
            Assert.Equal(50, warrior.Health);
        }

        [Fact]
        public void UseAbility_ByName_UsesDefaultMultiplier()
        {
            Character mage = Mage();
            Character warrior = Warrior();

            int damage = mage.UseAbility("Fireball", warrior);

            Assert.Equal(50, damage);
        }

        [Fact]
        public void RestoreState_SetsHealthAndStance()
        {
            Character warrior = Warrior();
            warrior.Attack(Mage());

            warrior.RestoreState(42, true);

            Assert.Equal(42, warrior.Health);
            Assert.True(warrior.IsDefending);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void RestoreState_OutOfRange_Throws(int health)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Warrior().RestoreState(health, false));
        }
    }
}
