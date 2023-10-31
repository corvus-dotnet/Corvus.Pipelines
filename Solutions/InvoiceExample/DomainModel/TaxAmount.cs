namespace InvoiceExample.DomainModel;

/// <summary>
/// A tax line item.
/// </summary>
/// <param name="TaxType">The type of the tax applied.</param>
/// <param name="Amount">The amount of tax applied.</param>
public record struct TaxAmount(string TaxType, decimal Amount);
