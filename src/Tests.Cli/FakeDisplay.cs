using Xunit;
using Infra.Display;

namespace Tests.Cli
{
    /// <summary>
    /// In-memory display double: queued inputs, captured outputs.
    /// Replaces console mocking without extra packages.
    /// </summary>
    public sealed class FakeDisplay : IDisplay
    {
        private readonly Queue<string?> _inputs;
        private readonly System.Text.StringBuilder _buffer = new();
        private readonly List<string> _lines = new();

        /// <summary>
        /// Gets all completed output lines.
        /// </summary>
        public IReadOnlyList<string> Lines => _lines;

        /// <summary>
        /// Initializes a fake with the given input lines.
        /// An empty queue means immediate end of input.
        /// </summary>
        /// <param name="inputs">The lines returned by ReadLine in order.</param>
        public FakeDisplay(params string?[] inputs)
        {
            _inputs = new Queue<string?>(inputs);
        }

        /// <summary>
        /// Gets whether input is redirected. Always true for fakes.
        /// </summary>
        public bool IsInputRedirected => true;

        /// <summary>
        /// Buffers a message without a new line.
        /// </summary>
        /// <param name="message">The message to buffer.</param>
        public void Write(string? message) => _buffer.Append(message);

        /// <summary>
        /// Flushes the buffer with the message as one line.
        /// </summary>
        /// <param name="message">The message to write, or null for the buffer only.</param>
        public void WriteLine(string? message = null)
        {
            _buffer.Append(message);
            _lines.Add(_buffer.ToString());
            _buffer.Clear();
        }

        /// <summary>
        /// Dequeues the next input line, or null when exhausted.
        /// </summary>
        /// <returns>The next line, or null.</returns>
        public string? ReadLine() =>
            _inputs.Count > 0 ? _inputs.Dequeue() : null;

        /// <summary>
        /// Checks whether any output line contains the given text.
        /// </summary>
        /// <param name="text">The text to find.</param>
        /// <returns>True when found.</returns>
        public bool Shows(string text) => _lines.Any(l => l.Contains(text, StringComparison.Ordinal));
    }
}
