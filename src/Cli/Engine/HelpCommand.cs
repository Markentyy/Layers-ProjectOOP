using Infra.Display;
namespace Cli.Engine
{
    /// <summary>
    /// Lists all registered commands with their usage lines.
    /// </summary>
    public sealed class HelpCommand : IShellCommand
    {
        private readonly CommandSet _commands;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "help";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Show available commands.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "help";

        /// <summary>
        /// Initializes a help command bound to the given command set.
        /// </summary>
        /// <param name="commands">The command set to describe.</param>
        public HelpCommand(CommandSet commands)
        {
            ArgumentNullException.ThrowIfNull(commands);
            _commands = commands;
        }

        /// <summary>
        /// Prints every registered command with its usage line.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line (ignored).</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            foreach (IShellCommand cmd in _commands.Commands)
            {
                console.WriteLine($"{cmd.Name} - {cmd.Summary}");
                console.WriteLine($"  Usage: {cmd.Usage}");
            }
            console.WriteLine("exit - Leave the interpreter.");
        }
    }
}
