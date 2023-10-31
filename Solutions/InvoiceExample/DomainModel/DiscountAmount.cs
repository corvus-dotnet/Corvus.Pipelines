namespace InvoiceExample.DomainModel;

/// <summary>
/// An amount for a discount, with the reason it has been applied.
/// </summary>
/// <param name="Reason">The reason for the discount.</param>
/// <param name="Amount">The amount of the discount.</param>
public record struct DiscountAmount(string Reason, decimal Amount);
