namespace BitcoinPrices.Domain.ValueObjects;

public sealed record Instrument
{
    public Instrument(Currency baseCurrency, Currency quoteCurrency)
    {
        if (baseCurrency == quoteCurrency)
        {
            throw new ArgumentException("Base and quote currencies must be different.");
        }

        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
    }

    public Currency BaseCurrency { get; }
    public Currency QuoteCurrency { get; }

    public static Instrument BtcUsd => new(Currency.Btc, Currency.Usd);

    public override string ToString() => $"{BaseCurrency}/{QuoteCurrency}";
}
