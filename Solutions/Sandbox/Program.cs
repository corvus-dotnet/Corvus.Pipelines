using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Corvus.YarpPipelines;
using Yarp.ReverseProxy.Transforms;
using PipelineExamples;
using Corvus.Pipelines;

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

PipelineStep<decimal> pipeline = Pipeline.Build(    
    Pipeline.Current<decimal>().Choose(
        selector: state => state > 1000m
            ? InvoiceSteps.ApplyHighDiscount 
            : InvoiceSteps.ApplyLowDiscount),
    InvoiceSteps.ApplySalesTax.ToAsync()
);

decimal output = await pipeline(1000m).ConfigureAwait(false);

Console.WriteLine(output);
Console.WriteLine();

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

