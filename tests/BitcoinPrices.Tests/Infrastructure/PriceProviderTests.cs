using System.Net;
using System.Text;
using BitcoinPrices.Domain.ValueObjects;
using BitcoinPrices.Infrastructure.Providers.Bitfinex;
using BitcoinPrices.Infrastructure.Providers.Bitstamp;

namespace BitcoinPrices.Tests.Infrastructure;

public sealed class PriceProviderTests
{
    private static readonly DateTimeOffset RequestedTime =
        new(2023, 1, 1, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Bitstamp_MapsRequestAndClosePrice()
    {
        var handler = new StubHandler(
            "{\"data\":{\"ohlc\":[{\"timestamp\":\"1672531200\",\"close\":\"16531.00\"}]}}");
        var provider = new BitstampPriceProvider(
            CreateClient(handler, "https://www.bitstamp.net/"),
            new FixedTimeProvider(RequestedTime));

        var result = await provider.GetPriceAsync(
            Instrument.BtcUsd,
            RequestedTime,
            CancellationToken.None);

        Assert.Equal(16531.00, result.Price, 8);
        Assert.Contains(
            "api/v2/ohlc/btcusd/?step=3600&limit=1&start=1672531200",
            handler.RequestUri!.ToString(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Bitfinex_MapsRequestAndClosePrice()
    {
        var handler = new StubHandler("[[1672531200000,16500.0,16529.5,16600.0,16400.0,10.0]]");
        var provider = new BitfinexPriceProvider(
            CreateClient(handler, "https://api-pub.bitfinex.com/"),
            new FixedTimeProvider(RequestedTime));

        var result = await provider.GetPriceAsync(
            Instrument.BtcUsd,
            RequestedTime,
            CancellationToken.None);

        Assert.Equal(16529.5, result.Price, 8);
        Assert.Contains(
            "v2/candles/trade:1h:tBTCUSD/hist?start=1672531200000&end=1672534800000&limit=1&sort=1",
            handler.RequestUri!.ToString(),
            StringComparison.Ordinal);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler, string baseAddress)
    {
        return new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
