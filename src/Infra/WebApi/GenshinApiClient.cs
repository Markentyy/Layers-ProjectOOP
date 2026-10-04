using System.Text.Json;

namespace Infra.WebApi
{
    /// <summary>
    /// HTTP implementation of the Genshin database client.
    /// </summary>
    public sealed class GenshinApiClient : IGenshinApiClient
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes a client over the given HTTP transport.
        /// </summary>
        /// <param name="http">The HTTP client with the API base address.</param>
        public GenshinApiClient(HttpClient http)
        {
            ArgumentNullException.ThrowIfNull(http);
            _http = http;
        }

        /// <summary>
        /// Lists all character ids of the database.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The character ids.</returns>
        public async Task<IReadOnlyList<string>> GetCharacterIdsAsync(CancellationToken token) =>
            await GetAsync<List<string>>("characters", token).ConfigureAwait(false);

        /// <summary>
        /// Reads one character with talents by id.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The character data.</returns>
        public async Task<GenshinCharacterDto> GetCharacterAsync(string id, CancellationToken token) =>
            await GetAsync<GenshinCharacterDto>($"characters/{id}", token, $"Unknown character '{id}'.").ConfigureAwait(false);

        /// <summary>
        /// Lists all weapon ids of the database.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The weapon ids.</returns>
        public async Task<IReadOnlyList<string>> GetWeaponIdsAsync(CancellationToken token) =>
            await GetAsync<List<string>>("weapons", token).ConfigureAwait(false);

        /// <summary>
        /// Reads one weapon with stats by id.
        /// </summary>
        /// <param name="id">The weapon id.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The weapon data.</returns>
        public async Task<GenshinWeaponDto> GetWeaponAsync(string id, CancellationToken token) =>
            await GetAsync<GenshinWeaponDto>($"weapons/{id}", token, $"Unknown weapon '{id}'.").ConfigureAwait(false);

        /// <summary>
        /// Gets and parses one endpoint, translating failures.
        /// </summary>
        /// <typeparam name="T">The expected payload type.</typeparam>
        /// <param name="path">The relative endpoint path.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="missingMessage">The message for HTTP 404, or null for lists.</param>
        /// <returns>The parsed payload.</returns>
        private async Task<T> GetAsync<T>(string path, CancellationToken token, string? missingMessage = null)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(path, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                throw new WebApiException($"Database is unreachable: {ex.Message}", ex);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && missingMessage is not null)
                throw new WebApiException(missingMessage);
            if (!response.IsSuccessStatusCode)
                throw new WebApiException($"Database answered {(int)response.StatusCode} on '{path}'.");

            try
            {
                string json = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                return JsonSerializer.Deserialize<T>(json, Options)
                    ?? throw new WebApiException($"Database sent an empty answer on '{path}'.");
            }
            catch (JsonException ex)
            {
                throw new WebApiException($"Database sent invalid data on '{path}'.", ex);
            }
        }
    }
}
