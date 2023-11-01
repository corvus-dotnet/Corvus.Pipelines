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
            options.SingleLine = false;
        });
    });

ILogger logger = loggerFactory.CreateLogger("Yarp pipeline logging");

string productId = "Catalog2_Product2";

HandlerState<string, decimal> pricingResult = PricingCatalogs.PricingHandler(HandlerState<string, decimal>.For(productId));

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

foreach (string path in paths)
{
    RequestTransformContext ctx = new() { HttpContext = new DefaultHttpContext() { Request = { Path = path } }, Path = path };

    YarpPipelineState result = await ExampleYarpPipelineWithLogging.Instance(YarpPipelineState.For(ctx, logger)).ConfigureAwait(false);

    if (result.ShouldForward(out NonForwardedResponseDetails responseDetails))
    {
        Console.WriteLine($"Forwarding, message is: {ctx.HttpContext.Items["Message"] ?? "not set"}");
    }
    else
    {
        Console.WriteLine($"Not forwarding: Status Code {responseDetails.StatusCode}");
    }

    Console.WriteLine();
    Console.WriteLine();
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

