namespace BitcoinPrices.Domain.ValueObjects;

public readonly record struct Currency
{
    public Currency(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (normalizedCode.Length is < 3 or > 10 ||
            normalizedCode.Any(character => !char.IsAsciiLetterOrDigit(character)))
        {
            throw new ArgumentException("Currency codes must contain 3-10 ASCII letters or digits.", nameof(code));
        }

        Code = normalizedCode;
    }

    public string Code { get; }

    public static Currency Btc => new("BTC");
    public static Currency Usd => new("USD");

    public override string ToString() => Code;
}
