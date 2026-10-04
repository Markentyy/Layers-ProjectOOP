using Xunit;
using Cli.Text;
using Cli.Engine;
using Cli.Presenter;
using Core.TextSystem;

namespace Tests.Cli
{
    public sealed class TextNavigatorTests
    {
        private static TextNavigator Seeded()
        {
            TextDocument document = new();
            Section oop = new("oop");
            oop.AddChild(new Heading(1, "H"));
            Section sub = new("sub");
            sub.AddChild(new Paragraph("P"));
            oop.AddChild(sub);
            document.AddElement(oop);
            return new TextNavigator(document);
        }

        [Fact]
        public void Pwd_TracksPosition()
        {
            TextNavigator nav = Seeded();

            Assert.Equal("/", nav.Pwd());
            nav.Cd("oop");
            Assert.Equal("/oop", nav.Pwd());
            nav.Cd("sub");
            Assert.Equal("/oop/sub", nav.Pwd());
            nav.Up();
            Assert.Equal("/oop", nav.Pwd());
        }

        [Fact]
        public void Up_AtRoot_Throws()
        {
            Assert.Throws<CommandException>(() => Seeded().Up());
        }

        [Fact]
        public void Cd_AbsoluteRelativeParentDot()
        {
            TextNavigator nav = Seeded();

            nav.Cd("/oop/sub");
            Assert.Equal("/oop/sub", nav.Pwd());
            nav.Cd("..");
            Assert.Equal("/oop", nav.Pwd());
            nav.Cd(".");
            Assert.Equal("/oop", nav.Pwd());
            nav.Cd("/oop/../oop/sub/");
            Assert.Equal("/oop/sub", nav.Pwd());
        }

        [Fact]
        public void Cd_UnknownOrLeaf_Throws()
        {
            TextNavigator nav = Seeded();

            Assert.Throws<CommandException>(() => nav.Cd("nope"));
            nav.Cd("oop");
            Assert.Throws<CommandException>(() => nav.Cd("H"));
        }

        [Fact]
        public void Cd_IsCaseInsensitive()
        {
            TextNavigator nav = Seeded();

            nav.Cd("OOP");

            Assert.Equal("/oop", nav.Pwd());
        }

        [Fact]
        public void CdById_JumpsToContainer_RejectsLeafAndUnknown()
        {
            TextNavigator nav = Seeded();
            Section oop = Assert.IsType<Section>(nav.CurrentChildren[0]);
            Section sub = Assert.IsType<Section>(oop.Children[1]);
            Heading leaf = Assert.IsType<Heading>(oop.Children[0]);

            nav.CdById(sub.Id);
            Assert.Equal("/oop/sub", nav.Pwd());

            Assert.Throws<CommandException>(() => nav.CdById(leaf.Id));
            Assert.Throws<CommandException>(() => nav.CdById("e999"));
        }

        [Fact]
        public void AddToCurrent_NumbersWithoutCollisions()
        {
            TextNavigator nav = Seeded();
            nav.Cd("oop");

            nav.AddToCurrent(new Paragraph("New"));

            Assert.Equal("e5", nav.CurrentChildren[2].Id);
        }

        [Fact]
        public void ReplaceDocument_ResetsPositionAndCounter()
        {
            TextNavigator nav = Seeded();
            nav.Cd("oop/sub");

            TextDocument fresh = new();
            fresh.AddElement(new Section("solo"));
            nav.ReplaceDocument(fresh);

            Assert.Equal("/", nav.Pwd());
            nav.AddToCurrent(new Paragraph("P"));
            Assert.Equal("e2", nav.CurrentChildren[1].Id);
        }

        [Fact]
        public void RemoveCurrent_RemovesSectionAndMovesUp()
        {
            TextNavigator nav = Seeded();
            nav.Cd("oop");

            Section removed = nav.RemoveCurrent();

            Assert.Equal("oop", removed.Title);
            Assert.Equal("/", nav.Pwd());
            Assert.Empty(nav.Document.Elements);
        }

        [Fact]
        public void RemoveCurrent_AtRoot_Throws()
        {
            Assert.Throws<CommandException>(() => Seeded().RemoveCurrent());
        }

        [Fact]
        public void FindInCurrent_MatchesDisplayName()
        {
            TextNavigator nav = Seeded();
            nav.Cd("oop");

            Assert.Single(nav.FindInCurrent("sub"));
            Assert.Single(nav.FindInCurrent("SUB"));
            Assert.Empty(nav.FindInCurrent("nope"));
        }

        [Fact]
        public void RenderCurrent_WithIds_MarksElements()
        {
            TextNavigator nav = Seeded();
            nav.Cd("oop/sub");

            string text = nav.RenderCurrent(true);

            Assert.Contains("[e4]", text);
            Assert.Contains("(Paragraph)", text);
        }
    }
}
