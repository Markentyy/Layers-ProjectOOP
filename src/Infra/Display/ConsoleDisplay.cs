namespace Infra.Display
{
    /// <summary>
    /// Default console implementation backed by the system console.
    /// </summary>
    public sealed class ConsoleDisplay : IDisplay
    {
        /// <summary>
        /// Writes a message to the console without a trailing new line.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Write(string? message) => Console.Write(message);

        /// <summary>
        /// Writes a message to the console followed by a new line.
        /// </summary>
        /// <param name="message">The message to write, or null for an empty line.</param>
        public void WriteLine(string? message = null) => Console.WriteLine(message);

        /// <summary>
        /// Reads one line from the console, or null on end of input.
        /// </summary>
        /// <returns>The entered line, or null on end of input.</returns>
        public string? ReadLine() => Console.ReadLine();

        /// <summary>
        /// Gets whether the input comes from a pipe or file instead of the keyboard.
        /// </summary>
        public bool IsInputRedirected => Console.IsInputRedirected;
    }
}
