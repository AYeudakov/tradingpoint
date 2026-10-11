using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Models;

public sealed record ProviderPrice(
    string ProviderName,
    Instrument Instrument,
    DateTimeOffset TimeUtc,
    double Price,
    DateTimeOffset RetrievedAtUtc);
