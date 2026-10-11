using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.ValueObjects;
using System.Text.Json;

namespace BitcoinPrices.Infrastructure.Providers.Bitfinex;

public sealed class BitfinexPriceProvider(HttpClient httpClient, TimeProvider timeProvider) : IPriceProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TimeProvider _timeProvider = timeProvider;

    public string Name => "Bitfinex";

    public Task<ProviderPrice> GetPriceAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        CancellationToken cancellationToken)
    {
        var candleStartUtc = timeUtc.AddHours(-1);
        var startMilliseconds = candleStartUtc.ToUnixTimeMilliseconds();
        var endMilliseconds = timeUtc.ToUnixTimeMilliseconds();
        var symbol = $"t{instrument.BaseCurrency.Code}{instrument.QuoteCurrency.Code}";
        var requestUri =
            $"v2/candles/trade:1h:{symbol}/hist?start={startMilliseconds}&end={endMilliseconds}&limit=1&sort=1";

        return FetchAsync(
            requestUri,
            instrument,
            timeUtc,
            startMilliseconds,
            cancellationToken);
    }

    private async Task<ProviderPrice> FetchAsync(
        string requestUri,
        Instrument instrument,
        DateTimeOffset timeUtc,
        long expectedTimestamp,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        // have you seen the response from it... why you make me to do that 
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array ||
            root.GetArrayLength() != 1 ||
            root[0].ValueKind != JsonValueKind.Array ||
            root[0].GetArrayLength() < 3 ||
            root[0][0].GetInt64() != expectedTimestamp)
        {
            throw new InvalidOperationException("Bitfinex returned an invalid candle.");
        }

        var close = root[0][2].GetDouble();
        if (!double.IsFinite(close) || close <= 0)
        {
            throw new InvalidOperationException("Bitfinex returned an invalid close price.");
        }

        return new ProviderPrice(Name, instrument, timeUtc, close, _timeProvider.GetUtcNow());
    }
}
