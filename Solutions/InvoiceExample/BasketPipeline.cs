using System.Diagnostics;
using Corvus.Pipelines;

namespace InvoiceExample;

public static class BasketPipeline
{
    public static PipelineStep<BasketState> Current = Pipeline.Current<BasketState>();
    public static SyncPipelineStep<BasketState> CurrentSync = Pipeline.CurrentSync<BasketState>();

    public static SyncPipelineStep<BasketState> ApplyLowDiscount = static state => state.ApplyDiscountFactor("20% bulk purchase discount", 0.2m);
    public static SyncPipelineStep<BasketState> ApplyHighDiscount = static state => state.ApplyDiscount("30% bulk purchase discount", 0.3m);
    public static SyncPipelineStep<BasketState> ApplyVAT = static state => state.ApplyTaxFactor("VAT", 0.2m);

    public static SyncPipelineStep<BasketState> ApplyDiscountPolicy =
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
