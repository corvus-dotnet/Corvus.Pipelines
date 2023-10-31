namespace InvoiceExample.Services;

/// <summary>
/// The prices for items as provided by supplier one.
/// </summary>
public class SupplierTwoPricingService
{
    /// <summary>
    /// Gets the price for an item based on its product range ID and SKU.
    /// </summary>
    /// <param name="range">The range ID for the item.</param>
    /// <param name="sku">The sku for the item.</param>
    /// <returns>The price for a single unit of the SKU, or null if the supplier cannot provide the item.</returns>
    public Task<decimal?> GetPrice(string range, string sku)
    {
        return Task.FromResult<decimal?>(
            range switch
            {
                "main" => sku switch
                {
                    "Item1" => 9.99m,
                    "Item2" => 7.50m,
                    "Item3" => 8.80m,
                    _ => null,
                },
                "own-brand" => sku switch
                {
                    "Item1" => 8.99m,
                    "Item2" => 6.50m,
                    "Item3" => 7.80m,
                    _ => null,
                },
                _ => null
            });
    }
}
