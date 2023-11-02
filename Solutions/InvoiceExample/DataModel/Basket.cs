
using System.Collections.Immutable;

namespace InvoiceExample.DataModel;

/// <summary>
/// A basket of items.
/// </summary>
/// <param name="LineItems">The items in the basket.</param>
public readonly record struct Basket(ImmutableArray<BasketItem> LineItems);
