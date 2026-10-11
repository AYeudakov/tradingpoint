using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Models;

public sealed record AggregatedPriceResponse(
    string Instrument,
    DateTimeOffset TimeUtc,
    string Currency,
    double AggregatedPrice,
    ResultSource ResultSource,
    AggregationMethod AggregationMethod,
    int AggregationVersion,
    IReadOnlyCollection<SourcePriceResponse> SourcePrices)
{
    public static AggregatedPriceResponse Map(
        AggregatedPrice price,
        ResultSource resultSource)
    {
        var instrument = new Instrument(price.BaseCurrency, price.QuoteCurrency);
        return new AggregatedPriceResponse(
            instrument.ToString(),
            price.TimeUtc,
            price.QuoteCurrency.Code,
            price.Price,
            resultSource,
            price.AggregationMethod,
            price.AggregationVersion,
            price.Sources
                .OrderBy(source => source.ProviderName)
                .Select(source => new SourcePriceResponse(
                    source.ProviderName,
                    source.Price))
                .ToArray());
    }
}
