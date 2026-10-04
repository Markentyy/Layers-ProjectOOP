using Cli.Engine;
using Xunit;
using Cli.Chars;
using Cli.Text;
using Core.GameSystem;
using Core.TextSystem;
using Infra.Data;

namespace Tests.Cli
{
    public sealed class MapperTests
    {
        [Fact]
        public void CharsWorld_RoundtripsThroughSnapshot()
        {
            // Arrange
            CharsWorld world = new();
            world.Characters.Add(new Character("A", 100, 5, 15), "a");
            world.Items.Add(new Equipment("S", 10, 0), "s");
            world.Abilities.Add(new Ability("F", 2), "f");
            world.Characters.GetById("a").Equip(world.Items.GetById("s"));
            world.Learn("a", "f");
            world.Characters.GetById("a").Attack(world.Characters.GetById("a"));

            // Act
            CharsSnapshot snapshot = CharsWorldMapper.ToSnapshot(world);
            CharsWorld restored = new();
            CharsWorldMapper.LoadInto(restored, snapshot);

            // Assert
            Character c = restored.ResolveCharacter("a").Value;
            Assert.Equal(80, c.Health);
            Assert.Equal(25, c.TotalAttack);
            Assert.Equal(new[] { "f" }, restored.BookOf("a"));
            Assert.Equal("a", Assert.Single(restored.HoldersOfAbility("f")).Key);
        }

        [Fact]
        public void CharsWorld_LoadInto_UnknownRef_ThrowsCommandError()
        {
            // Arrange
            CharsWorld world = new();
            CharsSnapshot snapshot = new();
            snapshot.Characters.Add(new CharacterState
            {
                Id = "a", Name = "A", MaxHealth = 100, Health = 100,
                BaseArmor = 0, BaseAttack = 1, ItemIds = { "ghost" },
            });

            // Act & Assert
            CommandException ex = Assert.Throws<CommandException>(
                () => CharsWorldMapper.LoadInto(world, snapshot));
            Assert.Contains("ghost", ex.Message);
        }

        [Fact]
        public void Text_RoundtripsThroughSnapshot_KeepsIds()
        {
            // Arrange
            TextDocument document = new();
            Section outer = new("Outer");
            outer.AddChild(new Heading(2, "H"));
            Section inner = new("Inner");
            inner.AddChild(new Paragraph("P"));
            inner.AddChild(new Link("L", "http://x.io"));
            outer.AddChild(inner);
            document.AddElement(outer);
            TextNavigator nav = new(document);
            string before = nav.RenderWhole(true);

            // Act
            TextSnapshot snapshot = TextMapper.ToSnapshot(document);
            TextDocument rebuilt = TextMapper.ToDocument(snapshot);
            string after = new TextNavigator(rebuilt).RenderWhole(true);

            // Assert
            Assert.Equal(before, after);
            Section rebuiltOuter = Assert.IsType<Section>(rebuilt.Elements[0]);
            Assert.Equal("e1", rebuiltOuter.Id);
            Assert.IsType<Section>(rebuiltOuter.Children[1]);
        }

        [Fact]
        public void Text_ToDocument_UnknownKind_Throws()
        {
            // Arrange
            TextSnapshot snapshot = new();
            snapshot.Roots.Add(new TextNode { Id = "e1", Kind = "portal" });

            // Act & Assert
            Assert.Throws<CommandException>(() => TextMapper.ToDocument(snapshot));
        }
    }
}
