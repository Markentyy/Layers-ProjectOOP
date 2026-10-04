using Xunit;
using Infra.Data;

namespace Tests.Infra
{
    public sealed class CharsStoreTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }

        private static CharsSnapshot Sample() =>
            new()
            {
                Characters =
                {
                    new CharacterState
                    {
                        Id = "char-1", Name = "A", MaxHealth = 100, Health = 85,
                        BaseArmor = 5, BaseAttack = 15, IsDefending = true,
                        ItemIds = { "item-1" }, AbilityIds = { "ability-1" },
                    },
                },
                Items = { new ItemState { Id = "item-1", Name = "S", AttackBonus = 5, ArmorBonus = 0 } },
                Abilities = { new AbilityState { Id = "ability-1", Name = "F", DamageMultiplier = 2 } },
            };

        [Fact]
        public void SaveLoad_RoundtripsAllFields()
        {
            JsonCharsStore store = new();
            string path = Path.Combine(_dir, "chars.json");
            Directory.CreateDirectory(_dir);

            store.Save(Sample(), path);
            CharsSnapshot loaded = store.Load(path);

            Assert.True(File.Exists(path));
            CharacterState c = Assert.Single(loaded.Characters);
            Assert.Equal("char-1", c.Id);
            Assert.Equal(85, c.Health);
            Assert.True(c.IsDefending);
            Assert.Equal(new[] { "item-1" }, c.ItemIds);
            Assert.Equal(new[] { "ability-1" }, c.AbilityIds);
            Assert.Equal(5, Assert.Single(loaded.Items).AttackBonus);
            Assert.Equal(2, Assert.Single(loaded.Abilities).DamageMultiplier);
        }

        [Fact]
        public void Load_MissingFile_ThrowsStoreException()
        {
            JsonCharsStore store = new();

            Assert.Throws<StoreException>(() => store.Load(Path.Combine(_dir, "nope.json")));
        }

        [Fact]
        public void Load_CorruptContent_ThrowsStoreException()
        {
            JsonCharsStore store = new();
            Directory.CreateDirectory(_dir);
            string path = Path.Combine(_dir, "bad.json");
            File.WriteAllText(path, "not json{{{");

            Assert.Throws<StoreException>(() => store.Load(path));
        }

        [Fact]
        public void Save_MissingDirectory_ThrowsStoreException()
        {
            JsonCharsStore store = new();

            Assert.Throws<StoreException>(() => store.Save(Sample(), Path.Combine(_dir, "nope", "x.json")));
        }
    }
}
