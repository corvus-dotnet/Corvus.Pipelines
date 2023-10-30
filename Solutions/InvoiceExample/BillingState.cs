using System.Diagnostics.CodeAnalysis;

namespace InvoiceExample;
public readonly struct BillingState
{
    private readonly decimal? totalToPay;

    private BillingState(Invoice invoice, decimal? totalToPay)
    {
        this.Invoice = invoice;
        this.totalToPay = totalToPay;
    }

    /// <summary>
    /// Gets the invoice for the billing state.
    /// </summary>
    public Invoice Invoice { get; }

    public static BillingState For(Invoice invoice)
    {
        return new(invoice, null);
    }

    /// <summary>
    /// Try to get the total to pay after processing.
    /// </summary>
    /// <param name="totalToPay">The total to pay.</param>
    /// <returns><see langword="true"/> if the total was calculated successfully,
    /// otherwise <see langword="false"/>.</returns>
    public bool TryGetTotalToPay([NotNullWhen(true)] out decimal totalToPay)
    {
        if (this.totalToPay is decimal total)
        {
            totalToPay = total;
            return true;
        }

        totalToPay = default;
        return false;
    }
}
