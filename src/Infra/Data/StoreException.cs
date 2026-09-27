namespace Infra.Data
{
    /// <summary>
    /// Error raised when a state file cannot be written, read or parsed.
    /// Commands translate it into a user facing command error.
    /// </summary>
    public sealed class StoreException : Exception
    {
        /// <summary>
        /// Initializes a store error with the given message.
        /// </summary>
        /// <param name="message">The error text.</param>
        public StoreException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a store error with the given message and cause.
        /// </summary>
        /// <param name="message">The error text.</param>
        /// <param name="inner">The underlying I/O or format error.</param>
        public StoreException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
