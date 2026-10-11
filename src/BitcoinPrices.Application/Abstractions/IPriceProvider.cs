using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Abstractions;

public interface IPriceProvider
{
    string Name { get; }

    Task<ProviderPrice> GetPriceAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        CancellationToken cancellationToken);
}
