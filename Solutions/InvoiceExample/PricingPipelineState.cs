using InvoiceExample.DataModel;

namespace InvoiceExample;

/// <summary>
/// Prices a basket item by providing the best 
/// </summary>
public readonly struct PricingPipelineState
{
    private readonly BasketItem basketItem;
    private readonly (string SupplierId, decimal Price)? bestPrice;

    private PricingPipelineState(in BasketItem basketItem, (string SupplierId, decimal Price)? bestPrice)
    {
        this.basketItem = basketItem;
        this.bestPrice = bestPrice;
    }

    /// <summary>
    /// Try to get the best price for the item.
    /// </summary>
    /// <param name="bestPrice">The best price for the item</param>
    /// <returns><see langword="true"/> if a best price was found for the item.</returns>
    public bool TryGetBestPrice(out (string SupplierId, decimal Price) bestPrice)
    {
        bestPrice = this.bestPrice ?? default;
        return this.bestPrice is not null;
    }

    public PricingPipelineState UpdateBestPrice(string supplierId, decimal bestPrice)
    {
        if (this.bestPrice is (string, decimal) bp)
        {
            return bp.Price <= bestPrice ? this : new(this.basketItem, (supplierId, bestPrice));
        }

        return new(this.basketItem, (supplierId, bestPrice));
    }
}