using Xunit;
using Cli.Engine;

namespace Tests.Cli
{
    public sealed class ParserTests
    {
        [Fact]
        public void Tokenize_SplitsOnWhitespace()
        {
            List<string> tokens = LineParser.Tokenize("act attack A B");

            Assert.Equal(new[] { "act", "attack", "A", "B" }, tokens);
        }

        [Fact]
        public void Tokenize_KeepsQuotedPartsTogether()
        {
            List<string> tokens = LineParser.Tokenize("add heading \"my title here\"");

            Assert.Equal(new[] { "add", "heading", "my title here" }, tokens);
        }

        [Fact]
        public void Tokenize_EmptyQuotesBecomeEmptyToken()
        {
            List<string> tokens = LineParser.Tokenize("ls \"\"");

            Assert.Equal(new[] { "ls", "" }, tokens);
        }

        [Fact]
        public void Tokenize_UnterminatedQuote_Throws()
        {
            Assert.Throws<CommandException>(() => LineParser.Tokenize("add \"oops"));
        }

        [Fact]
        public void Parse_LowercasesVerb_AndReadsOptions()
        {
            ParsedCommand cmd = LineParser.Parse("ADD --char_id A --id=B --whole");

            Assert.Equal("add", cmd.Verb);
            Assert.True(cmd.HasOption("char_id"));
            Assert.Equal("A", cmd.GetOption("char_id"));
            Assert.Equal("B", cmd.GetOption("id"));
            Assert.Null(cmd.GetOption("whole"));
            Assert.Null(cmd.GetOption("missing"));
        }

        [Fact]
        public void Parse_EmptyLine_Throws()
        {
            Assert.Throws<CommandException>(() => LineParser.Parse("   "));
        }

        [Fact]
        public void Parse_BracketPlaceholder_SuggestsFixedLine()
        {
            CommandException ex = Assert.Throws<CommandException>(
                () => LineParser.Parse("add --char_id <A> --id <B>"));

            Assert.Contains("Wrong format", ex.Message);
            Assert.Contains("add --char_id A --id B", ex.Message);
        }

        [Fact]
        public void RequireOption_Missing_Throws()
        {
            ParsedCommand cmd = LineParser.Parse("add --char_id A");

            Assert.Throws<CommandException>(() => cmd.RequireOption("id"));
        }

        [Fact]
        public void RequireArg_Missing_Throws()
        {
            ParsedCommand cmd = LineParser.Parse("act attack");

            Assert.Throws<CommandException>(() => cmd.RequireArg(2, "a target."));
        }

        [Fact]
        public void ExpectArgCount_OutsideRange_Throws()
        {
            ParsedCommand cmd = LineParser.Parse("ls char extra");

            Assert.Throws<CommandException>(() => cmd.ExpectArgCount(1, 1, "usage"));
        }

        [Fact]
        public void ExpectOptions_Unknown_Throws()
        {
            ParsedCommand cmd = LineParser.Parse("print --hole");

            Assert.Throws<CommandException>(() => cmd.ExpectOptions("usage", "whole", "id"));
        }
    }
}
