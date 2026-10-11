using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Aggregation;
using BitcoinPrices.Application.Configuration;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Application.Services;
using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BitcoinPrices.Tests.Application;

public sealed class PriceServiceTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2023, 1, 2, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RequestedTime =
        new(2023, 1, 1, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAggregatedPrice_WhenStored_ReturnsCacheWithoutCallingProviders()
    {
        var repository = new FakeRepository { StoredPrice = CreateStoredPrice() };
        var provider = new FakeProvider("Unused", 1);
        var service = CreateService(repository, provider);

        var result = await service.GetAggregatedPriceAsync(
            Instrument.BtcUsd,
            RequestedTime,
            AggregationMethod.ArithmeticMean,
            CancellationToken.None);

        Assert.Equal(ResultSource.Datastore, result.ResultSource);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task GetAggregatedPrice_WhenMissing_FetchesConcurrentlyAndPersistsSources()
    {
        var repository = new FakeRepository();
        var gate = new ProviderGate(2);
        var providers = new IPriceProvider[]
        {
            new FakeProvider("Bitstamp", 16531.00, gate),
            new FakeProvider("Bitfinex", 16529.50, gate)
        };
        var service = CreateService(repository, providers);

        var result = await service.GetAggregatedPriceAsync(
            Instrument.BtcUsd,
            RequestedTime,
            AggregationMethod.ArithmeticMean,
            CancellationToken.None);

        Assert.Equal(ResultSource.ExternalApis, result.ResultSource);
        Assert.Equal(16530.25, result.AggregatedPrice, 8);
        Assert.Equal(2, result.SourcePrices.Count);
        Assert.NotNull(repository.AddedPrice);
        Assert.Equal(2, repository.AddedPrice.Sources.Count);
    }

    [Fact]
    public async Task GetAggregatedPrice_RejectsTimeThatIsNotHourAligned()
    {
        var service = CreateService(new FakeRepository());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAggregatedPriceAsync(
                Instrument.BtcUsd,
                RequestedTime.AddMinutes(1),
                AggregationMethod.ArithmeticMean,
                CancellationToken.None));
    }

    private static PriceService CreateService(
        FakeRepository repository,
        params IPriceProvider[] providers)
    {
        var aggregator = new ArithmeticMeanPriceAggregator();
        return new PriceService(
            providers,
            new PriceAggregatorResolver(new[] { aggregator }),
            repository,
            Options.Create(new AggregationOptions { MinimumSuccessfulProviders = 2 }),
            new FixedTimeProvider(CurrentTime),
            NullLogger<PriceService>.Instance);
    }

    private static AggregatedPrice CreateStoredPrice()
    {
        return new AggregatedPrice
        {
            BaseCurrency = Currency.Btc,
            QuoteCurrency = Currency.Usd,
            TimeUtc = RequestedTime,
            Price = 16530.25,
            AggregationMethod = AggregationMethod.ArithmeticMean,
            AggregationVersion = 1,
            CreatedAtUtc = CurrentTime,
            Sources = new[]
            {
                new SourcePrice
                {
                    Id = 1,
                    ProviderName = "Bitstamp",
                    Price = 16531,
                    RetrievedAtUtc = CurrentTime
                },
                new SourcePrice
                {
                    Id = 2,
                    ProviderName = "Bitfinex",
                    Price = 16529.5,
                    RetrievedAtUtc = CurrentTime
                }
            }
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ProviderGate(int expectedCount)
    {
        private int _arrivalCount;
        private readonly TaskCompletionSource _allArrived =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task ArriveAsync()
        {
            if (Interlocked.Increment(ref _arrivalCount) == expectedCount)
            {
                _allArrived.TrySetResult();
            }

            await _allArrived.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
    }

    private sealed class FakeProvider(
        string name,
        double price,
        ProviderGate? gate = null) : IPriceProvider
    {
        public string Name => name;
        public int CallCount { get; private set; }

        public async Task<ProviderPrice> GetPriceAsync(
            Instrument instrument,
            DateTimeOffset timeUtc,
            CancellationToken cancellationToken)
        {
            CallCount++;
            if (gate is not null)
            {
                await gate.ArriveAsync();
            }

            return new ProviderPrice(Name, instrument, timeUtc, price, CurrentTime);
        }
    }

    private sealed class FakeRepository : IPriceRepository
    {
        public AggregatedPrice? StoredPrice { get; init; }
        public AggregatedPrice? AddedPrice { get; private set; }

        public Task<AggregatedPrice?> FindAggregateAsync(
            Instrument instrument,
            DateTimeOffset timeUtc,
            AggregationMethod aggregationMethod,
            int aggregationVersion,
            CancellationToken cancellationToken) => Task.FromResult(StoredPrice);

        public Task<IReadOnlyCollection<AggregatedPrice>> GetAggregatesAsync(
            Instrument instrument,
            DateTimeOffset fromUtc,
            DateTimeOffset toUtc,
            AggregationMethod aggregationMethod,
            int aggregationVersion,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<AggregatedPrice>>(Array.Empty<AggregatedPrice>());

        public Task AddAggregateAsync(
            AggregatedPrice aggregatedPrice,
            CancellationToken cancellationToken)
        {
            AddedPrice = aggregatedPrice;
            return Task.CompletedTask;
        }
    }
}
