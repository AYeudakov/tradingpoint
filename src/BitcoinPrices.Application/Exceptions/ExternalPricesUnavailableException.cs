namespace BitcoinPrices.Application.Exceptions;

public sealed class ExternalPricesUnavailableException(
    int successfulProviderCount,
    int requiredProviderCount)
    : Exception(
        $"Only {successfulProviderCount} price provider(s) succeeded; " +
        $"at least {requiredProviderCount} are required.")
{
    public int SuccessfulProviderCount { get; } = successfulProviderCount;
    public int RequiredProviderCount { get; } = requiredProviderCount;
}
