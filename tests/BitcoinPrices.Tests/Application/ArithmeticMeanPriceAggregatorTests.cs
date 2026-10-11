using BitcoinPrices.Application.Aggregation;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Tests.Application;

public sealed class ArithmeticMeanPriceAggregatorTests
{
    private readonly ArithmeticMeanPriceAggregator _aggregator = new();

    [Fact]
    public void Aggregate_ReturnsArithmeticMean()
    {
        var time = new DateTimeOffset(2023, 1, 1, 1, 0, 0, TimeSpan.Zero);
        var prices = new[]
        {
            new ProviderPrice("Bitstamp", Instrument.BtcUsd, time, 16531.00, time),
            new ProviderPrice("Bitfinex", Instrument.BtcUsd, time, 16529.50, time)
        };

        var result = _aggregator.Aggregate(prices);

        Assert.Equal(16530.25, result, 8);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Aggregate_RejectsInvalidPrice(double price)
    {
        var time = new DateTimeOffset(2023, 1, 1, 1, 0, 0, TimeSpan.Zero);
        var prices = new[]
        {
            new ProviderPrice("Provider", Instrument.BtcUsd, time, price, time)
        };

        Assert.Throws<ArgumentException>(() => _aggregator.Aggregate(prices));
    }
}
