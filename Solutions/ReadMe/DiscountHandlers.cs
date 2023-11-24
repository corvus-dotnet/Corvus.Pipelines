using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;

static class DiscountHandlers
{
    public static SyncPipelineStep<HandlerState<decimal, SyncPipelineStep<decimal>>> HandleHighDiscount =
        state => state.Input > 1000m ? state.Handled(InvoiceSteps.ApplyHighDiscount) : state.NotHandled();

    public static SyncPipelineStep<HandlerState<decimal, SyncPipelineStep<decimal>>> HandleLowDiscount =
        state => state.Input > 500m ? state.Handled(InvoiceSteps.ApplyLowDiscount) : state.NotHandled();
}
