using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InvoiceExample.DataModel;
using InvoiceExample.DomainModel;

namespace InvoiceExample;

public readonly struct BasketState
{
    private readonly IServiceProvider serviceProvider;
    private readonly BasketSummary basketSummary;
    private readonly State calculationState;

    private BasketState(in Basket basket, in BasketSummary basketSummary, State calculationState, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.Basket = basket;
        this.serviceProvider = serviceProvider;
        this.basketSummary = basketSummary;
        this.calculationState = calculationState;
    }

    /// <summary>
    /// Gets the basket for the basket state.
    /// </summary>
    public Basket Basket { get; }

    /// <summary>
    /// Gets the current subtotal for the basket.
    /// </summary>
    public decimal Subtotal => this.basketSummary.Subtotal;

    /// <summary>
    /// Gets the basket state for the given basket.
    /// </summary>
    /// <param name="basket">The basket for which to get the processing state.</param>
    /// <param name="serviceProvider">THe service provider for the pipeline context.</param>
    /// <returns>An instance of the basket state.</returns>
    public static BasketState For(in Basket basket, IServiceProvider serviceProvider)
    {
        return new(basket, default, State.NotCalculated, serviceProvider);
    }

    /// <summary>
    /// Try to get the summary pricing calculation for the basket.
    /// </summary>
    /// <param name="totalToPay">The total to pay.</param>
    /// <returns><see langword="true"/> if the total was calculated successfully,
    /// otherwise <see langword="false"/>.</returns>
    public bool TryGetPricingSummary([NotNullWhen(true)] out BasketSummary summary)
    {
        summary = this.basketSummary;
        return this.calculationState != State.NotCalculated;
    }

    /// <summary>
    /// Sets the subtotal for the basket.
    /// </summary>
    /// <param name="discount">The subtotal to set.</param>
    /// <returns>The updated basket state.</returns>
    /// <remarks>
    /// You must finish setting the subtotal, before applying any discounts or adding tax calculations.
    /// </remarks>
    public BasketState WithSubtotal(decimal subtotal)
    {
        Debug.Assert(this.calculationState == State.NotCalculated || this.calculationState == State.CalculatedSubtotal, "You must finish calculating a subtotal before applying any discounts or taxes.");
        return new(this.Basket, BasketSummary.From(subtotal), State.CalculatedSubtotal, this.serviceProvider);
    }

    /// <summary>
    /// Sets the subtotal for the basket.
    /// </summary>
    /// <param name="discount">The value to add to the subtotal.</param>
    /// <returns>The updated basket state.</returns>
    /// <remarks>
    /// You must finish setting the subtotal, before applying any discounts or adding tax calculations.
    /// </remarks>
    public BasketState AddToSubtotal(decimal delta)
    {
        Debug.Assert(this.calculationState == State.NotCalculated || this.calculationState == State.CalculatedSubtotal, "You must finish calculating a subtotal before applying any discounts or taxes.");
        return new(this.Basket, BasketSummary.From(this.basketSummary.Subtotal + delta), State.CalculatedSubtotal, this.serviceProvider);
    }

    /// <summary>
    /// Applies a discount factor to the basket state.
    /// </summary>
    /// <param name="discount">The discount factor to apply.</param>
    /// <returns>The updated basket state.</returns>
    /// <remarks>
    /// You must apply discounts after setting the subtotal, and before adding tax calculations.
    /// </remarks>
    public BasketState ApplyDiscountFactor(string reason, decimal discountFactor)
    {
        Debug.Assert(this.calculationState != State.NotCalculated, "You must calculate a total before applying a discount.");
        Debug.Assert(this.calculationState != State.CalculatedTax, "You must calculate all discounts before calculating tax.");
        return new(this.Basket, this.basketSummary.ApplyDiscountFactor(reason, discountFactor), State.CalculatedDiscount, this.serviceProvider);
    }

    /// <summary>
    /// Applies an absolute discount to the basket state.
    /// </summary>
    /// <param name="discount">The absolute discount to apply.</param>
    /// <returns>The updated basket state.</returns>
    /// <remarks>
    /// This invalidates the sales tax, so you should apply the sales tax after applying the discount.
    /// </remarks>
    public BasketState ApplyDiscount(string reason, decimal discount)
    {
        Debug.Assert(this.calculationState != State.NotCalculated, "You must calculate a total before applying a discount.");
        Debug.Assert(this.calculationState != State.CalculatedTax, "You must calculate all discounts before calculating tax.");
        return new(this.Basket, this.basketSummary.ApplyDiscount(reason, discount), State.CalculatedDiscount, this.serviceProvider);
    }

    /// <summary>
    /// Applies a sales tax factor to the basket state.
    /// </summary>
    /// <param name="taxFactor">The tax factor to apply.</param>
    /// <returns>The updated basket state.</returns>
    /// <remarks>
    /// This takes account of the discount, so you should apply the sales tax after applying the discount.
    /// </remarks>
    public BasketState ApplyTaxFactor(string taxType, decimal taxFactor)
    {
        Debug.Assert(this.calculationState != State.NotCalculated, "You must calculate a total before applying a discount.");
        return new(this.Basket, this.basketSummary.ApplyTaxFactor(taxType, taxFactor), State.CalculatedTax, this.serviceProvider);
    }


    private enum State
    {
        /// <summary>
        /// The pipeline has not yet calculated a subtotal for the basket.
        /// </summary>
        NotCalculated,
        
        /// <summary>
        /// A subtotal has been calculated for the basket.
        /// </summary>
        CalculatedSubtotal,

        /// <summary>
        /// A discount has been calculated for the basket.
        /// </summary>
        CalculatedDiscount,

        /// <summary>
        /// The tax has been calculated for the basket.
        /// </summary>
        CalculatedTax,
    }

}
