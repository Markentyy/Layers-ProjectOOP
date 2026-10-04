namespace Infra.WebApi
{
    /// <summary>
    /// Reads characters and weapons from the remote Genshin database.
    /// Depends on an injected HttpClient, so tests can substitute the transport.
    /// </summary>
    public interface IGenshinApiClient
    {
        /// <summary>
        /// Lists all character ids of the database.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The character ids.</returns>
        Task<IReadOnlyList<string>> GetCharacterIdsAsync(CancellationToken token);

        /// <summary>
        /// Reads one character with talents by id.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The character data.</returns>
        Task<GenshinCharacterDto> GetCharacterAsync(string id, CancellationToken token);

        /// <summary>
        /// Lists all weapon ids of the database.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The weapon ids.</returns>
        Task<IReadOnlyList<string>> GetWeaponIdsAsync(CancellationToken token);

        /// <summary>
        /// Reads one weapon with stats by id.
        /// </summary>
        /// <param name="id">The weapon id.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The weapon data.</returns>
        Task<GenshinWeaponDto> GetWeaponAsync(string id, CancellationToken token);
    }
}
