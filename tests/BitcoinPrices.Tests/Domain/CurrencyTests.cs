using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Tests.Domain;

public sealed class CurrencyTests
{
    [Fact]
    public void Constructor_NormalizesCode()
    {
        var currency = new Currency(" btc ");

        Assert.Equal("BTC", currency.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("US$")]
    public void Constructor_RejectsInvalidCode(string code)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Currency(code));
    }
}
