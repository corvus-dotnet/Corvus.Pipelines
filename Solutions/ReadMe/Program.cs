using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;

SyncPipelineStep<int> addOne = static state => state + 1;

int result = addOne(1);

Console.WriteLine(result);

SyncPipelineStep<int> syncPipeline = Pipeline.Build<int>(
    static state => state + 1,
    static state => state * 2,
    static state => state - 1);

result = syncPipeline(1);

Console.WriteLine(result);

PipelineStep<int> asyncPipeline = Pipeline.Build<int>(
    static async state =>
    {
        await Task.Delay(1000).ConfigureAwait(false);
        return state * 2;
    });

result = await asyncPipeline(2);

asyncPipeline = Pipeline.Build<int>(
    static state => ValueTask.FromResult(state + 1),
    static async state => { await Task.Delay(0); return state * 2; },
    static state => ValueTask.FromResult(state - 1));

result = await asyncPipeline(1);

Console.WriteLine(result);

SyncPipelineStep<int> terminatingPipeline = Pipeline.Build(
    shouldTerminate: state => state > 25,
    CommonSteps.MultiplyBy5,
    CommonSteps.MultiplyBy5
);

result = terminatingPipeline(1);

Console.WriteLine(result);

result = terminatingPipeline(6);

Console.WriteLine(result);


SyncPipelineStep<decimal> chooseDiscount =
    Pipeline.Choose(
            selector: static (decimal state) =>
            state switch
            {
                > 1000m => InvoiceSteps.ApplyHighDiscount,
                > 500m => InvoiceSteps.ApplyLowDiscount,
                _ => Pipeline.CurrentSync<decimal>(),
            });

SyncPipelineStep<decimal> invoicePipeline =
    Pipeline.Build(
        chooseDiscount,
        InvoiceSteps.ApplySalesTax);

decimal value = invoicePipeline(1000m);

Console.WriteLine(value);

value = invoicePipeline(2000m);

Console.WriteLine(value);

value = invoicePipeline(100m);

Console.WriteLine(value);

string productId = "Catalog2_Product2";

HandlerState<string, decimal> pricingResult =
    PricingCatalogs.PricingHandler(
        HandlerState<string, decimal>.For(productId));

Console.Write(productId);
Console.Write(" ");

if (pricingResult.WasHandled(out decimal price))
{
    Console.WriteLine(price);
}
else
{
    Console.WriteLine("was not priced");
}

SyncPipelineStep<ProductPrice> lookupProductPrice =
    PricingCatalogs.PricingHandler.Bind(
        (ProductPrice state) =>
            HandlerState<string, decimal>.For(state.ProductId),
        (outerState, innerState) =>
            new ProductPrice(
                innerState.Input,
                innerState.WasHandled(out decimal result) 
                    ? result : null));

SyncPipelineStep<ProductPrice> discountProductPrice =
    invoicePipeline.Bind(
        (ProductPrice state) => state.Price ?? 0m,
        (outerState, innerState) => new ProductPrice(outerState.ProductId, innerState));

SyncPipelineStep<ProductPrice> lookupPriceAndDiscount = 
    lookupProductPrice.Bind(discountProductPrice);

ProductPrice productPricingResult = lookupPriceAndDiscount(new ProductPrice(productId, null));

Console.WriteLine(productPricingResult);

productPricingResult = lookupPriceAndDiscount(new ProductPrice("You won't find me!", null));

Console.WriteLine(productPricingResult);

SyncPipelineStep<ProductPrice> saferDiscountProductPrice =
    invoicePipeline.Bind(
        (ProductPrice state) => state.Price ?? throw new InvalidOperationException("The base price was null."),
        (outerState, innerState) => new ProductPrice(outerState.ProductId, innerState));

SyncPipelineStep<ProductPrice> saferLookupPriceAndDiscount =
    lookupProductPrice.Bind(saferDiscountProductPrice);

productPricingResult = saferLookupPriceAndDiscount(new ProductPrice(productId, null));

Console.WriteLine(productPricingResult);

try
{
    productPricingResult = saferLookupPriceAndDiscount(new ProductPrice("You won't find me!", null));
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}

SyncPipelineStep<ProductPrice> safestLookupPriceAndDiscount =
    saferLookupPriceAndDiscount.Catch(
        (ProductPrice state, InvalidOperationException ex) => new (state.ProductId, null));


productPricingResult = safestLookupPriceAndDiscount(new ProductPrice("You won't find me!", null));

Console.WriteLine(productPricingResult);

public readonly record struct ProductPrice(string ProductId, decimal? Price);

static class CommonSteps
{
    public static SyncPipelineStep<int> MultiplyBy5 =
        state => state * 5;
}

static class InvoiceSteps
{
    public static SyncPipelineStep<decimal> ApplyLowDiscount =
        state => Math.Ceiling(state * 100 * 0.8m) / 100;
    public static SyncPipelineStep<decimal> ApplyHighDiscount =
        state => Math.Ceiling(state * 100 * 0.7m) / 100;
    public static SyncPipelineStep<decimal> ApplySalesTax =
        state => Math.Ceiling(state * 100 * 1.2m) / 100;
}

static class PricingCatalogs
{
    public static SyncPipelineStep<HandlerState<string, decimal>>
        PricingCatalog1 =
            state =>
            {
                return state.Input switch
                {
                    "Catalog1_Product1" => state.Handled(99.99m),
                    "Catalog1_Product2" => state.Handled(20.99m),
                    _ => state.NotHandled(),
                };
            };

    public static SyncPipelineStep<HandlerState<string, decimal>>
        PricingCatalog2 =
            state =>
            {
                return state.Input switch
                {
                    "Catalog2_Product1" => state.Handled(1.99m),
                    "Catalog2_Product2" => state.Handled(3.99m),
                    _ => state.NotHandled(),
                };
            };

    public static SyncPipelineStep<HandlerState<string, decimal>>
        PricingCatalog3 =
            state =>
            {
                return state.Input switch
                {
                    "Catalog3_ProductA" => state.Handled(12.99m),
                    "Catalog3_ProductB" => state.Handled(21.99m),
                    _ => state.NotHandled(),
                };
            };

    public static SyncPipelineStep<HandlerState<string, decimal>>
        PricingHandler =
            HandlerPipeline.Build(
                PricingCatalog1,
                PricingCatalog2,
                PricingCatalog3);
}