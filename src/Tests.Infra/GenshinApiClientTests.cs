using Xunit;
using Infra.WebApi;

namespace Tests.Infra
{
    public sealed class GenshinApiClientTests : IDisposable
    {
        private readonly List<HttpMessageHandler> _handlers = new();

        public void Dispose()
        {
            foreach (HttpMessageHandler handler in _handlers)
                handler.Dispose();
        }

        private GenshinApiClient Stub(string json, System.Net.HttpStatusCode code = System.Net.HttpStatusCode.OK)
        {
            StubHandler handler = new(_ => new HttpResponseMessage(code)
            {
                Content = new StringContent(json),
            });
            _handlers.Add(handler);
            return new GenshinApiClient(new HttpClient(handler) { BaseAddress = new Uri("http://test/") });
        }

        [Fact]
        public async Task GetCharacterIds_ParsesList()
        {
            GenshinApiClient client = Stub("[\"albedo\",\"klee\"]");

            // Act
            IReadOnlyList<string> ids = await client.GetCharacterIdsAsync(CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "albedo", "klee" }, ids);
        }

        [Fact]
        public async Task GetCharacter_ParsesLoreAndTalents()
        {
            GenshinApiClient client = Stub(
                "{\"name\":\"Albedo\",\"title\":\"Kreideprinz\",\"vision\":\"Geo\"," +
                "\"weapon\":\"Sword\",\"nation\":\"Mondstadt\",\"rarity\":5," +
                "\"description\":\"A genius.\"," +
                "\"skillTalents\":[{\"name\":\"Weiss\",\"unlock\":\"Normal Attack\"," +
                "\"description\":\"Strikes.\",\"type\":\"NORMAL_ATTACK\"," +
                "\"attribute-scaling\":[{\"name\":\"1-Hit\",\"value\":\"36%\"}]}]}");

            // Act
            GenshinCharacterDto dto = await client.GetCharacterAsync("albedo", CancellationToken.None);

            // Assert
            Assert.Equal("Albedo", dto.Name);
            Assert.Equal(5, dto.Rarity);
            GenshinTalentDto talent = Assert.Single(dto.SkillTalents);
            Assert.Equal("Weiss", talent.Name);
            Assert.Equal("NORMAL_ATTACK", talent.Type);
        }

        [Fact]
        public async Task GetWeapon_ParsesStats()
        {
            GenshinApiClient client = Stub(
                "{\"id\":\"skyward-blade\",\"name\":\"Skyward Blade\",\"type\":\"Sword\"," +
                "\"rarity\":5,\"baseAttack\":44,\"subStat\":\"Energy Recharge\"," +
                "\"passiveName\":\"Fang\",\"passiveDesc\":\"Desc.\"}");

            // Act
            GenshinWeaponDto dto = await client.GetWeaponAsync("skyward-blade", CancellationToken.None);

            // Assert
            Assert.Equal("Skyward Blade", dto.Name);
            Assert.Equal(44, dto.BaseAttack);
            Assert.Equal(5, dto.Rarity);
        }

        [Fact]
        public async Task GetCharacter_MissingId_ThrowsKnownMessage()
        {
            GenshinApiClient client = Stub("{\"error\":\"nope\"}", System.Net.HttpStatusCode.NotFound);

            // Act & Assert
            WebApiException ex = await Assert.ThrowsAsync<WebApiException>(
                () => client.GetCharacterAsync("ghost", CancellationToken.None));
            Assert.Contains("ghost", ex.Message);
        }

        [Fact]
        public async Task ServerError_ThrowsWebApiException()
        {
            GenshinApiClient client = Stub("oops", System.Net.HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<WebApiException>(() => client.GetWeaponIdsAsync(CancellationToken.None));
        }

        [Fact]
        public async Task GarbageBody_ThrowsWebApiException()
        {
            GenshinApiClient client = Stub("not json{{{");

            // Act & Assert
            await Assert.ThrowsAsync<WebApiException>(() => client.GetCharacterIdsAsync(CancellationToken.None));
        }

        [Fact]
        public async Task UnreachableHost_ThrowsWebApiException()
        {
            StubHandler handler = new(_ => throw new HttpRequestException("down"));
            _handlers.Add(handler);
            GenshinApiClient client = new(new HttpClient(handler) { BaseAddress = new Uri("http://test/") });

            // Act & Assert
            await Assert.ThrowsAsync<WebApiException>(() => client.GetCharacterIdsAsync(CancellationToken.None));
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(_responder(request));
        }
    }
}
