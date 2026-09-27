namespace Infra.Display
{
    /// <summary>
    /// Console abstraction used by commands and the REPL loop.
    /// Keeps command code testable and free of direct Console calls.
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// Writes a message without a trailing new line.
        /// </summary>
        /// <param name="message">The message to write.</param>
        void Write(string? message);

        /// <summary>
        /// Writes a message followed by a new line.
        /// </summary>
        /// <param name="message">The message to write, or null for an empty line.</param>
        void WriteLine(string? message = null);

        /// <summary>
        /// Reads one input line, or null on end of input.
        /// </summary>
        /// <returns>The entered line, or null on end of input.</returns>
        string? ReadLine();

        /// <summary>
        /// Gets whether the input comes from a pipe or file instead of the keyboard.
        /// Piped lines are echoed back so scripted runs stay readable.
        /// </summary>
        bool IsInputRedirected { get; }
    }
}
