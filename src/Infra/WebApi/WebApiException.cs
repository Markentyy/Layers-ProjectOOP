namespace Infra.WebApi
{
    /// <summary>
    /// Error raised when the remote database cannot be reached,
    /// answers with failure or returns unparsable data.
    /// Commands translate it into a user facing command error.
    /// </summary>
    public sealed class WebApiException : Exception
    {
        /// <summary>
        /// Initializes a web API error with the given message.
        /// </summary>
        /// <param name="message">The error text.</param>
        public WebApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a web API error with the given message and cause.
        /// </summary>
        /// <param name="message">The error text.</param>
        /// <param name="inner">The underlying transport error.</param>
        public WebApiException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
