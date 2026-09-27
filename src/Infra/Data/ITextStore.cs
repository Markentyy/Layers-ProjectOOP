namespace Infra.Data
{
    /// <summary>
    /// Persists text snapshots. Implementations are replaceable
    /// without touching commands: depend on this interface, not on JSON.
    /// </summary>
    public interface ITextStore
    {
        /// <summary>
        /// Writes the snapshot to the given file.
        /// </summary>
        /// <param name="snapshot">The snapshot to write.</param>
        /// <param name="path">The destination file path.</param>
        void Save(TextSnapshot snapshot, string path);

        /// <summary>
        /// Reads a snapshot from the given file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The loaded snapshot.</returns>
        TextSnapshot Load(string path);
    }
}
