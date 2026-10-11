using System.Runtime.CompilerServices;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Validation;

public static class ValidationExtensions
{
    public static void EnsureSupported(
        this Instrument? instrument,
        [CallerArgumentExpression(nameof(instrument))]
        string? parameterName = null)
    {
        ArgumentNullException.ThrowIfNull(instrument, parameterName);

        if (instrument != Instrument.BtcUsd)
        {
            throw new ArgumentException(
                "Only the BTC/USD instrument is currently supported.",
                parameterName);
        }
    }

    public static void EnsureUtcHour(
        this DateTimeOffset time,
        [CallerArgumentExpression(nameof(time))]
        string? parameterName = null)
    {
        if (time.Offset != TimeSpan.Zero ||
            time.Minute != 0 ||
            time.Second != 0 ||
            time.Millisecond != 0 ||
            time.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            throw new ArgumentException(
                "Time values must be UTC and aligned to an exact hour.",
                parameterName);
        }
    }
}
