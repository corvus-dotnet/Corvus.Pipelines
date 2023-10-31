namespace InvoiceExample.Services;

/// <summary>
/// The prices for items as provided by supplier one.
/// </summary>
public class SupplierOnePricingService
{
    /// <summary>
    /// Gets the price for an item based on its SKU.
    /// </summary>
    /// <param name="sku">The sku for the item.</param>
    /// <returns>The price for a single unit of the SKU, or null if the supplier cannot provide the item.</returns>
    public Task<decimal?> GetPrice(string sku)
    {
        return Task.FromResult<decimal?>(
            sku switch
            {
                "Item1" => 9.99m,
                "Item2" => 7.50m,
                "Item3" => 8.80m,
                _ => null,
            });
    }
}
