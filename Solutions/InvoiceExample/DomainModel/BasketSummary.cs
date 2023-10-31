
using System.Collections.Immutable;

namespace InvoiceExample.DomainModel;

/// <summary>
/// A summary of the basket value.
/// </summary>
/// <param name="Subtotal">The subtotal of the basket before tax and discount.</param>
/// <param name="Discount">The discount to apply.</param>
/// <param name="SalesTax">The sales tax to apply.</param>
public readonly struct BasketSummary
{
    private BasketSummary(decimal subtotal, in ImmutableArray<DiscountAmount> discounts, in ImmutableArray<TaxAmount> taxes)
    {
        this.Subtotal = subtotal;
        this.Discounts = discounts;
        this.Taxes = taxes;
    }

    public decimal TotalToPay => this.Subtotal - this.TotalDiscount() + this.TotalTaxes();

    /// <summary>
    /// Gets the subtotal for the basket.
    /// </summary>

    public decimal Subtotal { get; }

    /// <summary>
    /// Gets the discount amount for the basket.
    /// </summary>
    public ImmutableArray<DiscountAmount> Discounts { get; }

    /// <summary>
    /// Gets the sales tax amounts applied to the basket.
    /// </summary>
    public ImmutableArray<TaxAmount> Taxes { get; }

    /// <summary>
    /// Create a basket summary from a given subtotal.
    /// </summary>
    /// <param name="subtotal"></param>
    /// <returns></returns>
    public static BasketSummary From(decimal subtotal)
    {
        return new(subtotal, ImmutableArray<DiscountAmount>.Empty, ImmutableArray<TaxAmount>.Empty);
    }

    /// <summary>
    /// Apply an additional absolute value of discount, and reset the sales tax.
    /// </summary>
    /// <param name="reason">The reason for the discount.</param>
    /// <param name="discountFactor">The amount to apply as an additional discount./param>
    /// <returns>The updated basket summary.</returns>
    /// <remarks>
    /// <para>
    /// You can call <see cref="ApplyDiscount(string, decimal)"/> and <see cref="ApplyDiscountFactor(string, decimal)"/>
    /// multiple times to apply multiple discount values to the original <see cref="Subtotal"/>.
    /// </para>
    /// <para>
    /// See <see cref="ApplyDiscountFactor(string, decimal)"/> to apply a percentage discount to the basket.
    /// </para>
    /// </remarks>
    public BasketSummary ApplyDiscount(string reason, decimal discountAmount)
    {
        return new(this.Subtotal, this.Discounts.Add(new(reason, discountAmount)), ImmutableArray<TaxAmount>.Empty);
    }

    /// <summary>
    /// Apply an additional discount based on the original subtotal, and reset the sales tax.
    /// </summary>
    /// <param name="reason">The reason for the discount.</param>
    /// <param name="discountFactor">The factor to apply as an additional discount (e.g. 0.2m => 20%)</param>
    /// <returns>The updated basket summary.</returns>
    /// <remarks>
    /// <para>
    /// You can call <see cref="ApplyDiscount(string, decimal)"/> and <see cref="ApplyDiscountFactor(string, decimal)"/>
    /// multiple times to apply multiple discount values to the original <see cref="Subtotal"/>.
    /// </para>
    /// <para>
    /// See <see cref="ApplyDiscount(string, decimal)"/> to apply an absolute discount to the basket.
    /// </para>
    /// </remarks>
    public BasketSummary ApplyDiscountFactor(string reason, decimal discountFactor)
    {
        return new(this.Subtotal, this.Discounts.Add(new(reason, Money.MultiplyWithRounding(this.Subtotal, discountFactor))), ImmutableArray<TaxAmount>.Empty);
    }

    /// <summary>
    /// Apply an additional tax percentage of the given factor, based on the original subtotal.
    /// </summary>
    /// <param name="taxType">The type of the tax to apply.</param>
    /// <param name="factor">The factor to apply as a tax (e.g. 0.2m => 20%)</param>
    /// <returns>The updated basket summary.</returns>
    public BasketSummary ApplyTaxFactor(string taxType, decimal factor)
    {
        return new(this.Subtotal, this.Discounts, this.Taxes.Add(new(taxType, Money.MultiplyWithRounding(this.Subtotal - this.TotalDiscount(), factor))));
    }

    private decimal TotalTaxes()
    {
        return this.Taxes.Sum(s => s.Amount);
    }

    private decimal TotalDiscount()
    {
        return this.Discounts.Sum(s => s.Amount);
    }
}
