using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using BitcoinPrices.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BitcoinPrices.Tests.Infrastructure;

public sealed class PriceRepositoryTests
{
    [Fact]
    public async Task Repository_PersistsAggregateAndOwnedSourcePrices()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PriceDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new PriceDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        var repository = new PriceRepository(dbContext);
        var time = new DateTimeOffset(2023, 1, 1, 1, 0, 0, TimeSpan.Zero);
        var aggregate = new AggregatedPrice
        {
            BaseCurrency = Currency.Btc,
            QuoteCurrency = Currency.Usd,
            TimeUtc = time,
            Price = 16530.25,
            AggregationMethod = AggregationMethod.ArithmeticMean,
            AggregationVersion = 1,
            CreatedAtUtc = time,
            Sources = new[]
            {
                CreateSource("Bitstamp", 16531, time),
                CreateSource("Bitfinex", 16529.5, time)
            }
        };

        await repository.AddAggregateAsync(
            aggregate,
            CancellationToken.None);
        var stored = await repository.FindAggregateAsync(
            Instrument.BtcUsd,
            time,
            AggregationMethod.ArithmeticMean,
            1,
            CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(2, stored.Sources.Count);
        Assert.Equal(2, await dbContext.SourcePrices.CountAsync());
    }

    [Fact]
    public async Task ObservationMigration_PreservesExistingAggregateSources()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PriceDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new PriceDbContext(options);
        var migrator = dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync("20261010123724_InitialCreate");
        await dbContext.Database.ExecuteSqlRawAsync("""
            INSERT INTO AggregatedPrices
                (Id, BaseCurrency, QuoteCurrency, TimeUtc, Price,
                 AggregationMethod, AggregationVersion, CreatedAtUtc)
            VALUES
                (1, 'BTC', 'USD', 1672534800, 16530.25,
                 'ArithmeticMean', 1, 1672534800);

            INSERT INTO SourcePrices
                (AggregatedPriceId, ProviderName, Price, RetrievedAtUtc)
            VALUES
                (1, 'Bitstamp', 16531.0, 1672534800),
                (1, 'Bitfinex', 16529.5, 1672534800);
            """);

        await migrator.MigrateAsync();

        Assert.Equal(2, await dbContext.SourcePrices.CountAsync());
    }

    private static SourcePrice CreateSource(
        string providerName,
        double price,
        DateTimeOffset time)
    {
        return new SourcePrice
        {
            ProviderName = providerName,
            Price = price,
            RetrievedAtUtc = time
        };
    }
}
