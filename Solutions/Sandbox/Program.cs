using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Corvus.YarpPipelines;
using Yarp.ReverseProxy.Transforms;
using PipelineExamples;
using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;

string[] paths = ["/foo", "/bar", "/fizz", "/buzz", "/", "/baz"];

using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Trace);
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
        });
    });

ILogger logger = loggerFactory.CreateLogger("Yarp pipeline logging");

string productId = "Catalog2_Product2";

HandlerState<string, decimal> pricingResult = PricingCatalogs.PricingHandler(HandlerState<string, decimal>.For(productId));

if (pricingResult.WasHandled(out decimal price))
{
    logger.LogInformation("{productId}: {price}", productId, price);
}
else
{
    logger.LogInformation("{productId} not priced", productId);
}

foreach (string path in paths)
{
    RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = path } }, Path = path };

    YarpPipelineState result = ExampleYarpPipelineWithLogging.Instance(YarpPipelineState.For(ctx, logger));

    if (result.ShouldForward(out NonForwardedResponseDetails responseDetails))
    {
        logger.LogInformation("Forwarding, message is: {message}", ctx.HttpContext.Items["Message"] ?? "not set");
    }
    else
    {
        logger.LogInformation("Not forwarding: Status Code  {statusCode}", responseDetails.StatusCode);
    }
}

static class InvoiceSteps
{
    public static SyncPipelineStep<decimal> ApplyLowDiscount = state => Math.Ceiling(state * 100 * 0.8m) / 100;
    public static SyncPipelineStep<decimal> ApplyHighDiscount = state => Math.Ceiling(state * 100 * 0.7m) / 100;
    public static SyncPipelineStep<decimal> ApplySalesTax = state => Math.Ceiling(state * 100 * 1.2m) / 100;
}

static class PricingEngine
{
}


static class PricingCatalogs
{
    public static SyncPipelineStep<HandlerState<string, decimal>> PricingCatalog1 =
        state =>
        {
            return state.Input switch
            {
                "Catalog1_Product1" => state.Handled(99.99m),
                "Catalog1_Product2" => state.Handled(20.99m),
                _ => state.NotHandled(),
            };
        };

    public static SyncPipelineStep<HandlerState<string, decimal>> PricingCatalog2 =
        state =>
        {
            return state.Input switch
            {
                "Catalog2_Product1" => state.Handled(1.99m),
                "Catalog2_Product2" => state.Handled(3.99m),
                _ => state.NotHandled(),
            };
        };

    public static SyncPipelineStep<HandlerState<string, decimal>> PricingHandler =
        HandlerPipeline.Build(
            PricingCatalog1,
            PricingCatalog2);

}

