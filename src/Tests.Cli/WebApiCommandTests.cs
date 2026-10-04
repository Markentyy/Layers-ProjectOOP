using Xunit;
using Cli.Chars;
using Cli.Engine;
using Cli.Presenter;
using Cli.Text;
using Core.GameSystem;
using Infra.WebApi;

namespace Tests.Cli
{
    public sealed class WebApiCommandTests
    {
        private static (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api)
            Setup(params string?[] inputs)
        {
            CharsWorld world = new();
            FakeDisplay display = new(inputs);
            return (world, display, new CharsPresenter(display), new FakeGenshinApi());
        }

        private static ParsedCommand Cmd(string line) => LineParser.Parse(line);

        [Fact]
        public void Db_ListsCharactersAndWeapons()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api) = Setup();
            DbCommand db = new(api, presenter);

            // Act
            db.Execute(display, Cmd("db chars"));
            db.Execute(display, Cmd("db weapons"));

            // Assert
            Assert.True(display.Shows("[albedo] Albedo (5 stars, Geo Sword, Mondstadt)"));
            Assert.True(display.Shows("[skyward-blade] Skyward Blade (5 stars Sword, ATK 44)"));
        }

        [Fact]
        public void Db_SearchAndLimitAndBadCategory()
        {
            // Arrange
            FakeGenshinApi api = new();
            FakeDisplay searchDisplay = new();
            CharsPresenter searchPresenter = new(searchDisplay);
            FakeDisplay limitDisplay = new();
            CharsPresenter limitPresenter = new(limitDisplay);
            FakeDisplay errorDisplay = new();
            CharsPresenter errorPresenter = new(errorDisplay);

            // Act
            new DbCommand(api, searchPresenter).Execute(searchDisplay, Cmd("db chars --search klee"));
            new DbCommand(api, limitPresenter).Execute(limitDisplay, Cmd("db chars --limit 1"));

            // Assert
            Assert.True(searchDisplay.Shows("[klee] Klee"));
            Assert.False(searchDisplay.Shows("[albedo]"));
            Assert.True(limitDisplay.Shows("[albedo] Albedo"));
            Assert.False(limitDisplay.Shows("[klee]"));
            Assert.Throws<CommandException>(() => new DbCommand(api, errorPresenter).Execute(errorDisplay, Cmd("db potions")));
            Assert.Throws<CommandException>(() => new DbCommand(api, errorPresenter).Execute(errorDisplay, Cmd("db chars --limit nope")));
        }

        [Fact]
        public void Recruit_BuildsHeroWithTalentsAndLore()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api) = Setup();
            RecruitCommand recruit = new(world, api, presenter, new Random(42));

            // Act
            recruit.Execute(display, Cmd("recruit albedo"));

            // Assert
            Character hero = world.ResolveCharacter("albedo").Value;
            Assert.Equal("Albedo", hero.Name);
            Assert.InRange(hero.MaxHealth, 90, 120);
            Assert.Equal("Albedo", world.Lore["albedo"].Name);
            Assert.Equal(new[] { "albedo-t1", "albedo-t2", "albedo-t3" }, world.BookOf("albedo"));
            Assert.True(display.Shows("Recruited 'Albedo' [albedo]"));
            Assert.True(display.Shows("3 talents learned"));
        }

        [Fact]
        public void Recruit_UnknownOrDuplicate_Throws()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api) = Setup();
            RecruitCommand recruit = new(world, api, presenter, new Random(42));

            // Act & Assert
            Assert.Throws<CommandException>(() => recruit.Execute(display, Cmd("recruit ghost")));
            recruit.Execute(display, Cmd("recruit albedo"));
            Assert.Throws<CommandException>(() => recruit.Execute(display, Cmd("recruit albedo")));
        }

        [Fact]
        public void Fetch_ImportsWeapon()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api) = Setup();
            FetchCommand fetch = new(world, api, presenter);

            // Act
            fetch.Execute(display, Cmd("fetch skyward-blade"));

            // Assert
            Assert.Equal("Skyward Blade", world.ResolveItem("skyward-blade").Value.Name);
            Assert.True(display.Shows("Fetched 'Skyward Blade' [skyward-blade] (+4 ATK, +5 ARM)."));
            Assert.Throws<CommandException>(() => fetch.Execute(display, Cmd("fetch ghost-blade")));
        }

        [Fact]
        public void Show_PrintsSheetWithLore()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi api) = Setup();
            new RecruitCommand(world, api, presenter, new Random(42)).Execute(display, Cmd("recruit albedo"));
            world.Items.Add(new Core.GameSystem.Equipment("S", 1, 0), "s");
            world.Characters.GetById("albedo").Equip(world.Items.GetById("s"));

            // Act
            new ShowCommand(world, presenter).Execute(display, Cmd("show albedo"));

            // Assert
            string text = display.AllOutput();
            Assert.Contains("# Albedo", text);
            Assert.Contains("Kreideprinz", text);
            Assert.Contains("A genius alchemist", text);
            Assert.Contains("Isotoma", text);
            Assert.Contains("S [s] (+1 ATK, +0 ARM)", text);
        }

        [Fact]
        public void Show_WithoutLore_PrintsBasicSheet()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi _) = Setup();
            world.Characters.Add(new Character("Bob", 50, 1, 5), "bob");

            // Act
            new ShowCommand(world, presenter).Execute(display, Cmd("show bob"));

            // Assert
            Assert.Contains("hand-made hero", display.AllOutput());
            Assert.Contains("# Bob", display.AllOutput());
        }

        [Fact]
        public void Show_Unknown_Throws()
        {
            // Arrange
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter, FakeGenshinApi _) = Setup();

            // Act & Assert
            Assert.Throws<CommandException>(() => new ShowCommand(world, presenter).Execute(display, Cmd("show ghost")));
        }
    }
}
