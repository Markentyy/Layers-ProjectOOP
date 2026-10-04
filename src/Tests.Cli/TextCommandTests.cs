using Xunit;
using Cli.Text;
using Cli.Engine;
using Cli.Presenter;
using Core.TextSystem;
using Infra.Data;

namespace Tests.Cli
{
    public sealed class TextCommandTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }

        private static (TextNavigator nav, FakeDisplay display, TextPresenter presenter) Setup(params string?[] inputs)
        {
            TextDocument document = new();
            Section oop = new("oop");
            oop.AddChild(new Heading(1, "H"));
            document.AddElement(oop);
            TextNavigator nav = new(document);
            FakeDisplay display = new(inputs);
            return (nav, display, new TextPresenter(display));
        }

        private static ParsedCommand Cmd(string line) => LineParser.Parse(line);

        [Fact]
        public void Pwd_PrintsPath()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();

            new PwdCommand(nav, presenter).Execute(display, Cmd("pwd"));
            nav.Cd("oop");
            new PwdCommand(nav, presenter).Execute(display, Cmd("pwd"));

            Assert.True(display.Shows("/"));
            Assert.True(display.Shows("/oop"));
        }

        [Fact]
        public void Print_RendersCurrent_AndWholeWithIds()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            PrintCommand print = new(nav, presenter);

            print.Execute(display, Cmd("print"));
            print.Execute(display, Cmd("print --whole --id"));

            Assert.Contains("# oop", display.AllOutput());
            Assert.Contains("[e1] oop (Section)", display.AllOutput());
        }

        [Fact]
        public void Add_CreatesSectionAndLeaf()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup("Docs", "2", "Note", "Body");
            AddCommand add = new(nav, presenter);

            add.Execute(display, Cmd("add container section"));
            add.Execute(display, Cmd("add leaf heading"));

            Assert.True(display.Shows("Created Section 'Docs' [e3]"));
            Assert.True(display.Shows("Created Heading 'Note' [e4]"));
            Assert.Equal(3, nav.CurrentChildren.Count);
        }

        [Fact]
        public void Add_UnknownKindOrType_Throws()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            AddCommand add = new(nav, presenter);

            Assert.Throws<CommandException>(() => add.Execute(display, Cmd("add box section")));
            Assert.Throws<CommandException>(() => add.Execute(display, Cmd("add leaf table")));
            Assert.Throws<CommandException>(() => add.Execute(display, Cmd("add container page")));
        }

        [Fact]
        public void Rm_NamedChild_AsksConfirmation()
        {
            (TextNavigator nav, FakeDisplay displayYes, TextPresenter presenterYes) = Setup("y");
            (TextNavigator nav2, FakeDisplay displayNo, TextPresenter presenterNo) = Setup("n");
            foreach (TextNavigator n in new[] { nav, nav2 })
                n.AddToCurrent(new Paragraph("Temp"));

            new RmCommand(nav, presenterYes).Execute(displayYes, Cmd("rm Temp"));
            new RmCommand(nav2, presenterNo).Execute(displayNo, Cmd("rm Temp"));

            Assert.True(displayYes.Shows("Removed 'Temp'."));
            Assert.False(displayNo.Shows("Removed"));
            Assert.Equal(2, nav2.CurrentChildren.Count);
        }

        [Fact]
        public void Rm_UnknownOrAmbiguous_Throws()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            nav.AddToCurrent(new Paragraph("dup"));
            nav.AddToCurrent(new Paragraph("dup"));
            RmCommand rm = new(nav, presenter);

            Assert.Throws<CommandException>(() => rm.Execute(display, Cmd("rm nope")));
            Assert.Throws<CommandException>(() => rm.Execute(display, Cmd("rm dup")));
        }

        [Fact]
        public void Rm_Current_RemovesSectionAndMovesUp()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup("y");
            nav.Cd("oop");

            new RmCommand(nav, presenter).Execute(display, Cmd("rm"));

            Assert.True(display.Shows("Removed 'oop'. Current: /."));
            Assert.Equal("/", nav.Pwd());
        }

        [Fact]
        public void Up_MovesAndReports()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            nav.Cd("oop");

            new UpCommand(nav, presenter).Execute(display, Cmd("up"));

            Assert.True(display.Shows("Current: /."));
        }

        [Fact]
        public void Cd_NavigatesByPathAndId()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            CdCommand cd = new(nav, presenter);

            cd.Execute(display, Cmd("cd oop"));
            cd.Execute(display, Cmd("cd --id e1"));

            Assert.True(display.Shows("Current: /oop."));
        }

        [Fact]
        public void SaveLoad_RoundtripsDocument()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();
            Directory.CreateDirectory(_dir);
            string path = Path.Combine(_dir, "doc.json");
            JsonTextStore store = new();
            nav.AddToCurrent(new Paragraph("Keep me"));

            new SaveCommand(nav, store, presenter).Execute(display, Cmd($"save {path}"));
            nav.AddToCurrent(new Paragraph("Drop me"));
            new LoadCommand(nav, store, presenter).Execute(display, Cmd($"load {path}"));

            Assert.True(display.Shows($"Saved to '{path}'."));
            Assert.True(display.Shows($"Loaded from '{path}'."));
            Assert.Equal("/", nav.Pwd());
            Assert.DoesNotContain(nav.RenderWhole(false), "Drop me");
            Assert.Contains("Keep me", nav.RenderWhole(false));
        }

        [Fact]
        public void Load_MissingFile_ThrowsCommandError()
        {
            (TextNavigator nav, FakeDisplay display, TextPresenter presenter) = Setup();

            Assert.Throws<CommandException>(
                () => new LoadCommand(nav, new JsonTextStore(), presenter).Execute(display, Cmd("load nope.json")));
        }
    }
}
