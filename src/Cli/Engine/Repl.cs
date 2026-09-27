using Infra.Display;
namespace Cli.Engine
{
    /// <summary>
    /// Interactive read-eval loop shared by all interpreter modes.
    /// Reads lines behind a prompt and dispatches them to registered commands.
    /// </summary>
    public static class Repl
    {
        /// <summary>
        /// Runs the loop until the user types exit or the input ends.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="intro">The banner printed once before the first prompt.</param>
        /// <param name="commands">The commands available in this mode.</param>
        public static void Run(IDisplay console, string intro, CommandSet commands)
        {
            ArgumentNullException.ThrowIfNull(console);
            ArgumentNullException.ThrowIfNull(commands);
            console.WriteLine(intro);

            while (true)
            {
                console.Write("> ");
                string? line = console.ReadLine();
                if (line is null)
                    return;
                if (console.IsInputRedirected)
                    console.WriteLine(line);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                ParsedCommand parsed;
                try
                {
                    parsed = LineParser.Parse(line);
                }
                catch (CommandException ex)
                {
                    console.WriteLine($"Error: {ex.Message}");
                    continue;
                }

                if (parsed.Verb == "exit")
                    return;

                if (!commands.TryGet(parsed.Verb, out IShellCommand? cmd) || cmd is null)
                {
                    console.WriteLine($"Unknown command '{parsed.Verb}'. Type 'help'.");
                    continue;
                }

                try
                {
                    cmd.Execute(console, parsed);
                }
                catch (DialogCancelledException)
                {
                    console.WriteLine("Cancelled.");
                }
                catch (CommandException ex)
                {
                    console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
