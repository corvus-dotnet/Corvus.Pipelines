using Corvus.YarpPipelines;
using Microsoft.AspNetCore.Http;
using PipelineExamples;
using Yarp.ReverseProxy.Transforms;

RequestTransformContext[] Contexts =
    [
        new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/foo" } }, Path = "/foo" },
        new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/bar" } }, Path = "/bar" },
        new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/fizz" } }, Path = "/fizz" },
        new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/" } }, Path = "/" },
        new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/baz" } }, Path = "/baz" },
    ];

bool shouldForward = true;

// Run once to warm up the statics.
foreach (RequestTransformContext context in Contexts)
{
    YarpRequestPipelineState result = ExampleYarpPipeline.Instance(YarpRequestPipelineState.For(context));
    shouldForward &= result.ShouldForward(out ForwardedRequestDetails? requestDetails, out NonForwardedResponseDetails? responseDetails);
}


await Task.Delay(10000);

for (int i = 0; i < 8912; ++i)
{
    foreach (RequestTransformContext context in Contexts)
    {
        YarpRequestPipelineState result = ExampleYarpPipeline.Instance(YarpRequestPipelineState.For(context));
        shouldForward &= result.ShouldForward(out ForwardedRequestDetails? requestDetails, out NonForwardedResponseDetails? responseDetails);
    }
}
