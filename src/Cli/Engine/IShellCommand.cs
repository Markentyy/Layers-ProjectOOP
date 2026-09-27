using Infra.Display;
namespace Cli.Engine
{
    /// <summary>
    /// A single terminal command. New commands are added by implementing
    /// this interface and registering the instance in a command set.
    /// </summary>
    public interface IShellCommand
    {
        /// <summary>
        /// Gets the command verb used to invoke it.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Gets the detailed usage line shown by help.
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// Executes the command against the parsed line.
        /// </summary>
        /// <param name="console">The console for input and output.</param>
        /// <param name="command">The parsed command line.</param>
        void Execute(IDisplay console, ParsedCommand command);
    }
}
