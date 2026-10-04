using Xunit;
using Cli.Chars;
using Cli.Engine;
using Cli.Presenter;

namespace Tests.Cli
{
    public sealed class ReplTests
    {
        [Fact]
        public void Run_ExecutesKnown_ReportsUnknown_SkipsEmpty_Exits()
        {
            // Arrange
            CharsWorld world = new();
            FakeDisplay display = new("help", "", "nope", "exit");
            CommandSet set = CharsMode.CreateCommands(world, display, new Infra.Data.JsonCharsStore());

            // Act
            Repl.Run(display, "Intro.", set);

            // Assert
            Assert.True(display.Shows("Intro."));
            Assert.True(display.Shows("Usage: create <char|item|ability>"));
            Assert.True(display.Shows("Unknown command 'nope'. Type 'help'."));
        }

        [Fact]
        public void Run_EndOfInput_StopsQuietly()
        {
            // Arrange
            CharsWorld world = new();
            FakeDisplay display = new("ls char");
            CommandSet set = CharsMode.CreateCommands(world, display, new Infra.Data.JsonCharsStore());

            // Act
            Repl.Run(display, "Intro.", set);

            // Assert
            Assert.True(display.Shows("No characters."));
        }

        [Fact]
        public void Run_CommandError_PrintsError()
        {
            // Arrange
            CharsWorld world = new();
            FakeDisplay display = new("ls potion", "exit");
            CommandSet set = CharsMode.CreateCommands(world, display, new Infra.Data.JsonCharsStore());

            // Act
            Repl.Run(display, "Intro.", set);

            // Assert
            Assert.True(display.Shows("Error: Unknown category 'potion'."));
        }

        [Fact]
        public void Run_BrokenQuotes_PrintsError()
        {
            // Arrange
            CharsWorld world = new();
            FakeDisplay display = new("add \"oops", "exit");
            CommandSet set = CharsMode.CreateCommands(world, display, new Infra.Data.JsonCharsStore());

            // Act
            Repl.Run(display, "Intro.", set);

            // Assert
            Assert.True(display.Shows("Error: Unterminated quoted string."));
        }

        [Fact]
        public void CommandSet_DuplicateName_Throws()
        {
            // Arrange
            CommandSet set = new();
            CharsWorld world = new();
            FakeDisplay display = new();
            CharsPresenter presenter = new(display);

            // Act
            set.Add(new CreateCommand(world, presenter));

            // Assert
            Assert.Throws<InvalidOperationException>(() => { set.Add(new CreateCommand(world, presenter)); });
        }
    }
}
