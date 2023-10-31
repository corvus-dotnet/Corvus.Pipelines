
using System.Collections.Immutable;

namespace InvoiceExample.DataModel;

/// <summary>
/// A basket of items.
/// </summary>
/// <param name="LineItems">The items in the basket.</param>
public readonly record struct Basket(ImmutableArray<BasketItem> LineItems);

/// <summary>
/// An item in a basket.
/// </summary>
/// <param name="Sku">The unique item SKU.</param>
/// <param name="Description">The item description.</param>
/// <param name="Quantity">The number of units of the item SKU.</param>
public readonly record struct BasketItem(string Sku, string Description, int Quantity);