using Corvus.Pipelines;

namespace InvoiceExample;

public static class BasketPipeline
{
    public static readonly PipelineStep<BasketState> Current = Pipeline.Current<BasketState>();
    public static readonly SyncPipelineStep<BasketState> CurrentSync = Pipeline.CurrentSync<BasketState>();

    public static readonly SyncPipelineStep<BasketState> ApplyLowDiscount = static state => state.ApplyDiscountFactor("20% bulk purchase discount", 0.2m);
    public static readonly SyncPipelineStep<BasketState> ApplyHighDiscount = static state => state.ApplyDiscount("30% bulk purchase discount", 0.3m);
    public static readonly SyncPipelineStep<BasketState> ApplyVAT = static state => state.ApplyTaxFactor("VAT", 0.2m);

    public static readonly SyncPipelineStep<BasketState> ApplyDiscountPolicy =
        CurrentSync.Choose(
            state =>
            {
                if (state.Subtotal > 2000m)
                {
                    return ApplyHighDiscount;
                }
                else if (state.Subtotal > 1000m)
                {
                    return ApplyLowDiscount;
                }

                return CurrentSync;
            });
}