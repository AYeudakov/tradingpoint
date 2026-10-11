using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BitcoinPrices.Tests.Api;

public sealed class PriceApiTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"bitcoin-prices-tests-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program>? _factory;

    [Fact]
    public async Task History_WhenNoPricesExist_ReturnsEmptyDatastoreResult()
    {
        using var client = Factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/prices/history?baseCurrency=BTC&quoteCurrency=USD" +
            "&fromUtc=2023-01-01T00%3A00%3A00Z&toUtc=2023-01-02T00%3A00%3A00Z");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"resultSource\":\"datastore\"", body, StringComparison.Ordinal);
        Assert.Contains("\"prices\":[]", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Lookup_WhenTimeIsNotHourAligned_ReturnsBadRequest()
    {
        using var client = Factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/prices?baseCurrency=BTC&quoteCurrency=USD" +
            "&timeUtc=2023-01-01T01%3A01%3A00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        SqliteConnection.ClearAllPools();
        DeleteIfPresent(_databasePath);
        DeleteIfPresent($"{_databasePath}-shm");
        DeleteIfPresent($"{_databasePath}-wal");
    }

    private WebApplicationFactory<Program> Factory =>
        _factory ??= new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Prices"] = $"Data Source={_databasePath}"
                    });
                });
            });

    private static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
