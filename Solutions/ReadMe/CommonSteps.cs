using Corvus.Pipelines;

static class CommonSteps
{
    public static SyncPipelineStep<int> MultiplyBy5 =
        state => state * 5;
}
