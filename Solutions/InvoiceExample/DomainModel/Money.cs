namespace InvoiceExample.DomainModel;

public static class Money
{
    /// <summary>
    /// Multiply a decimal value by a multiplier, rounding up to 2 decimal places.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>The value multiplied by the multiplier, rounded up to 2 decimal places.</returns>
    public static decimal MultiplyWithRounding(decimal value, decimal multiplier)
    {
        return Round(value * multiplier);
    }

    /// <summary>
    /// Multiply a decimal value by a multiplier, truncating to 2 decimal places.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>The value multiplied by the multiplier, truncated to 2 decimal places.</returns>
    public static decimal MultiplyWithTruncation(decimal value, decimal multiplier)
    {
        return Truncate(value * multiplier);
    }

    /// <summary>
    /// Round up a decimal value to 2 decimal places.
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <returns></returns>
    public static decimal Round(decimal value)
    {
        return Math.Ceiling(value * 100) / 100;
    }

    /// <summary>
    /// Round up a decimal value to 2 decimal places.
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <returns></returns>
    public static decimal Truncate(decimal value)
    {
        return Math.Truncate(value * 100) / 100;
    }
}
