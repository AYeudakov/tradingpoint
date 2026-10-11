using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.ValueObjects;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BitcoinPrices.Infrastructure.Providers.Bitstamp;

public sealed class BitstampPriceProvider(HttpClient httpClient, TimeProvider timeProvider) : IPriceProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TimeProvider _timeProvider = timeProvider;

    public string Name => "Bitstamp";

    public Task<ProviderPrice> GetPriceAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        CancellationToken cancellationToken)
    {
        var candleStartUtc = timeUtc.AddHours(-1);
        var expectedTimestamp = candleStartUtc.ToUnixTimeSeconds();
        var symbol = $"{instrument.BaseCurrency.Code}{instrument.QuoteCurrency.Code}".ToLowerInvariant();
        var requestUri = $"api/v2/ohlc/{symbol}/?step=3600&limit=1&start={expectedTimestamp}";

        return FetchAsync(requestUri, instrument, timeUtc, expectedTimestamp, cancellationToken);
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

        var payload = await response.Content.ReadFromJsonAsync<BitstampResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Bitstamp returned an empty response.");
        var candle = payload.Data.Ohlc.SingleOrDefault()
            ?? throw new InvalidOperationException("Bitstamp returned no candle.");

        if (!long.TryParse(candle.Timestamp, CultureInfo.InvariantCulture, out var timestamp) ||
            timestamp != expectedTimestamp ||
            !double.TryParse(candle.Close, NumberStyles.Float, CultureInfo.InvariantCulture, out var close) ||
            !double.IsFinite(close) ||
            close <= 0)
        {
            throw new InvalidOperationException("Bitstamp returned an invalid candle.");
        }

        return new ProviderPrice(Name, instrument, timeUtc, close, _timeProvider.GetUtcNow());
    }

    private sealed class BitstampResponse
    {
        [JsonPropertyName("data")]
        public required BitstampData Data { get; init; }
    }

    private sealed class BitstampData
    {
        [JsonPropertyName("ohlc")]
        public required IReadOnlyCollection<BitstampCandle> Ohlc { get; init; }
    }

    private sealed class BitstampCandle
    {
        [JsonPropertyName("timestamp")]
        public required string Timestamp { get; init; }

        [JsonPropertyName("close")]
        public required string Close { get; init; }
    }
}
