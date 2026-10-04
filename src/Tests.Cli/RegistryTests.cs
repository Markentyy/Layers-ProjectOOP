using Cli.Engine;
using Xunit;
using Cli.Chars;
using Core.GameSystem;

namespace Tests.Cli
{
    public sealed class RegistryTests
    {
        [Fact]
        public void Add_AutoIdsGrowPerCategory()
        {
            CharsWorld world = new();

            string first = world.Characters.Add(new Character("A", 10, 0, 1), null);
            string second = world.Characters.Add(new Character("B", 10, 0, 1), null);

            Assert.Equal("char-1", first);
            Assert.Equal("char-2", second);
        }

        [Fact]
        public void Add_DuplicateId_Throws()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 10, 0, 1), "hero");

            Assert.Throws<CommandException>(
                () => world.Characters.Add(new Character("B", 10, 0, 1), "hero"));
        }

        [Fact]
        public void World_GlobalIds_UniqueAcrossRegistries()
        {
            CharsWorld world = new();
            world.Abilities.Add(new Ability("F", 2), "shared");

            Assert.Throws<CommandException>(
                () => world.AddItem(new Equipment("S", 1, 0), "shared"));
            Assert.True(world.IsIdTaken("shared"));
            Assert.False(world.IsIdTaken("free"));
        }

        [Fact]
        public void Resolve_ByIdAndByName()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("Arthur", 10, 0, 1), "a1");

            Assert.Equal("a1", world.ResolveCharacter("a1").Key);
            Assert.Equal("a1", world.ResolveCharacter("Arthur").Key);
            Assert.Equal("a1", world.ResolveCharacter("ARTHUR").Key);
        }

        [Fact]
        public void Resolve_Unknown_Throws()
        {
            Assert.Throws<CommandException>(() => new CharsWorld().ResolveCharacter("nope"));
        }

        [Fact]
        public void Resolve_DuplicateName_ThrowsAmbiguous()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("Same", 10, 0, 1), "c1");
            world.Characters.Add(new Character("Same", 10, 0, 1), "c2");

            CommandException ex = Assert.Throws<CommandException>(
                () => world.ResolveCharacter("Same"));
            Assert.Contains("c1", ex.Message);
            Assert.Contains("c2", ex.Message);
        }

        [Fact]
        public void Learn_IsIdempotent_AndTracksHolders()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 10, 0, 1), "a");
            world.Abilities.Add(new Ability("F", 2), "f");

            Assert.True(world.Learn("a", "f"));
            Assert.False(world.Learn("a", "f"));
            Assert.Equal(new[] { "f" }, world.BookOf("a"));
            Assert.Equal("a", Assert.Single(world.HoldersOfAbility("f")).Key);
            Assert.Empty(world.BookOf("ghost"));
        }

        [Fact]
        public void UsersOfItem_FindsCarriers()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 10, 0, 1), "a");
            world.Characters.Add(new Character("B", 10, 0, 1), "b");
            Equipment sword = new("S", 1, 0);
            world.Items.Add(sword, "s");
            world.Characters.GetById("a").Equip(sword);

            Assert.Equal("a", Assert.Single(world.UsersOfItem(sword)).Key);
        }

        [Fact]
        public void Clear_EmptiesEverything()
        {
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 10, 0, 1), "a");
            world.Learn("a", "f");

            world.Clear();

            Assert.Equal(0, world.Characters.Count);
            Assert.Empty(world.BookOf("a"));
        }
    }
}
