using System.Text.Json;

namespace Infra.Data
{
    /// <summary>
    /// JSON file implementation of the text store.
    /// </summary>
    public sealed class JsonTextStore : ITextStore
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        /// <summary>
        /// Writes the snapshot as indented JSON to the given file.
        /// </summary>
        /// <param name="snapshot">The snapshot to write.</param>
        /// <param name="path">The destination file path.</param>
        public void Save(TextSnapshot snapshot, string path)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            try
            {
                File.WriteAllText(path, JsonSerializer.Serialize(snapshot, Options));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new StoreException($"Cannot save to '{path}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Reads a snapshot from the given JSON file.
        /// </summary>
        /// <param name="path">The source file path.</param>
        /// <returns>The loaded snapshot.</returns>
        public TextSnapshot Load(string path)
        {
            try
            {
                TextSnapshot? snapshot = JsonSerializer.Deserialize<TextSnapshot>(File.ReadAllText(path), Options);
                return snapshot ?? throw new StoreException($"File '{path}' holds no snapshot.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                throw new StoreException($"Cannot load '{path}': {ex.Message}", ex);
            }
        }
    }
}
