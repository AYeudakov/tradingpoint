namespace BitcoinPrices.Application.Models;

public sealed record PriceRangeResponse(
    string Instrument,
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    ResultSource ResultSource,
    IReadOnlyCollection<AggregatedPriceResponse> Prices);
