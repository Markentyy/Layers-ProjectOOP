using Xunit;
using Cli.Chars;
using Cli.Engine;
using Cli.Presenter;
using Core.GameSystem;

namespace Tests.Cli
{
    public sealed class CharsCommandTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }

        private static (CharsWorld world, FakeDisplay display, CharsPresenter presenter) Setup(params string?[] inputs)
        {
            CharsWorld world = new();
            FakeDisplay display = new(inputs);
            return (world, display, new CharsPresenter(display));
        }

        private static ParsedCommand Cmd(string line) => LineParser.Parse(line);

        [Fact]
        public void Create_DialogBuildsCharacter()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) =
                Setup("Arthur", "100", "5", "15", "");

            new CreateCommand(world, presenter).Execute(display, Cmd("create char"));

            Assert.True(display.Shows("Created character 'Arthur' [char-1]"));
            Assert.Equal("Arthur", world.ResolveCharacter("char-1").Value.Name);
        }

        [Fact]
        public void Create_DuplicateId_ReportsError()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) =
                Setup("A", "10", "0", "1", "hero", "B", "10", "0", "1", "hero");
            CreateCommand create = new(world, presenter);

            create.Execute(display, Cmd("create char"));
            Assert.Throws<CommandException>(() => create.Execute(display, Cmd("create char")));
        }

        [Fact]
        public void Add_EquipsItem_AndRejectsDoubleEquip()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 5, 15), "a");
            world.Items.Add(new Equipment("S", 10, 0), "s");
            AddCommand add = new(world, presenter);

            add.Execute(display, Cmd("add --char_id a --id s"));
            add.Execute(display, Cmd("add --char_id a --id s"));

            Assert.True(display.Shows("A equipped S (+10 ATK, +0 ARM)."));
            Assert.True(display.Shows("A already has S equipped."));
            Assert.Equal(25, world.ResolveCharacter("a").Value.TotalAttack);
        }

        [Fact]
        public void Add_LearnsAbility_AndRejectsDoubleLearn()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 5, 15), "a");
            world.Abilities.Add(new Ability("F", 2), "f");
            AddCommand add = new(world, presenter);

            add.Execute(display, Cmd("add --char_id a --id f"));
            add.Execute(display, Cmd("add --char_id a --id f"));

            Assert.True(display.Shows("A learned F [f]."));
            Assert.True(display.Shows("A already knows F."));
        }

        [Fact]
        public void Add_UnknownTarget_Throws()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 5, 15), "a");

            Assert.Throws<CommandException>(
                () => new AddCommand(world, presenter).Execute(display, Cmd("add --char_id a --id ghost")));
        }

        [Fact]
        public void Act_Attack_ReportsDamage()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 0, 20), "a");
            world.Characters.Add(new Character("B", 100, 5, 1), "b");

            new ActCommand(world, presenter).Execute(display, Cmd("act attack a b"));

            Assert.True(display.Shows("A attacks B for 15 damage!"));
            Assert.Equal(85, world.ResolveCharacter("b").Value.Health);
        }

        [Fact]
        public void Act_Heal_DefaultsToFull()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 0, 20), "a");
            world.Characters.Add(new Character("B", 100, 0, 1), "b");
            ActCommand act = new(world, presenter);

            act.Execute(display, Cmd("act attack a b"));
            act.Execute(display, Cmd("act heal a b"));

            Assert.True(display.Shows("B heals for 20 HP. Current HP: 100/100"));
        }

        [Fact]
        public void Act_Heal_BadAmount_Throws()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 0, 1), "a");

            Assert.Throws<CommandException>(
                () => new ActCommand(world, presenter).Execute(display, Cmd("act heal a a --amount nope")));
        }

        [Fact]
        public void Act_Ability_RequiresLearning()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 0, 10), "a");
            world.Characters.Add(new Character("B", 100, 0, 1), "b");
            world.Abilities.Add(new Ability("F", 3), "f");
            ActCommand act = new(world, presenter);

            CommandException ex = Assert.Throws<CommandException>(
                () => act.Execute(display, Cmd("act ability a b --id f")));
            Assert.Contains("does not know", ex.Message);

            world.Learn("a", "f");
            act.Execute(display, Cmd("act ability a b --id f"));

            Assert.True(display.Shows("A uses special ability: [F] on B!"));
            Assert.True(display.Shows("It deals 30 damage!"));
        }

        [Fact]
        public void Act_Ability_AnnouncesDefeat()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 0, 100), "a");
            world.Characters.Add(new Character("B", 10, 0, 1), "b");
            world.Abilities.Add(new Ability("F", 2), "f");
            world.Learn("a", "f");

            new ActCommand(world, presenter).Execute(display, Cmd("act ability a b --id f"));

            Assert.True(display.Shows("B has been defeated!"));
        }

        [Fact]
        public void Ls_ListsAndDetails()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            world.Characters.Add(new Character("A", 100, 5, 15), "a");
            LsCommand ls = new(world, presenter);

            ls.Execute(display, Cmd("ls char"));
            ls.Execute(display, Cmd("ls item"));
            ls.Execute(display, Cmd("ls char --id a"));

            Assert.True(display.Shows("[a] A - 100/100 HP"));
            Assert.True(display.Shows("No items. Create one with: create item"));
            Assert.True(display.Shows("Attack: 15 base, 15 total"));
        }

        [Fact]
        public void Ls_UnknownCategory_Throws()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();

            Assert.Throws<CommandException>(
                () => new LsCommand(world, presenter).Execute(display, Cmd("ls potion")));
        }

        [Fact]
        public void SaveLoad_RoundtripsWorld()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();
            Directory.CreateDirectory(_dir);
            string path = Path.Combine(_dir, "w.json");
            world.Characters.Add(new Character("A", 100, 5, 15), "a");
            world.Items.Add(new Equipment("S", 10, 0), "s");
            SaveCommand save = new(world, new Infra.Data.JsonCharsStore(), presenter);

            save.Execute(display, Cmd($"save {path}"));
            world.Characters.GetById("a").Attack(world.Characters.GetById("a"));
            new LoadCommand(world, new Infra.Data.JsonCharsStore(), presenter).Execute(display, Cmd($"load {path}"));

            Assert.True(display.Shows($"Saved to '{path}'."));
            Assert.True(display.Shows("1 characters, 1 items, 0 abilities."));
            Assert.Equal(100, world.ResolveCharacter("a").Value.Health);
        }

        [Fact]
        public void Load_MissingFile_ThrowsCommandError()
        {
            (CharsWorld world, FakeDisplay display, CharsPresenter presenter) = Setup();

            Assert.Throws<CommandException>(
                () => new LoadCommand(world, new Infra.Data.JsonCharsStore(), presenter)
                    .Execute(display, Cmd("load nope.json")));
        }
    }
}
