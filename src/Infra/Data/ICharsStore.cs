namespace Infra.Data
{
    /// <summary>
    /// Persists characters snapshots. Implementations are replaceable
    /// without touching commands: depend on this interface, not on JSON.
    /// </summary>
    public interface ICharsStore
    {
        /// <summary>
        /// Writes the snapshot to the given file.
        /// </summary>
        /// <param name="snapshot">The snapshot to write.</param>
        /// <param name="path">The destination file path.</param>
        void Save(CharsSnapshot snapshot, string path);

        /// <summary>
        /// Reads a snapshot from the given file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The loaded snapshot.</returns>
        CharsSnapshot Load(string path);
    }
}
