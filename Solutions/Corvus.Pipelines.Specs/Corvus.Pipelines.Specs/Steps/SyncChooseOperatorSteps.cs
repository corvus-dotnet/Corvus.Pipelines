// <copyright file="SyncChooseOperatorSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class SyncChooseOperatorSteps(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext scenarioContext = scenarioContext;

    private Func<int, SyncPipelineStep<int>>? selector;
    private SyncPipelineStep<int>? step;

    [Given("I create a synchronous selector with the following configuration:")]
    public void GivenICreateASelectorWithTheFollowingConfiguration(Table table)
    {
        this.selector = SharedSteps.BuildSyncSelector<int>(table.Rows.Select(r => (r["Match"], r["Pipeline step"])));
    }

    [When("I produce a synchronous step by calling the Choose\\(\\) method with the selector")]
    public void WhenIProduceAStepByCallingTheChooseMethodWithTheSelector()
    {
        this.step = Pipeline.Choose<int>(this.Selector);
    }

    [When("I execute the synchronous step with the input (.*)")]
    public void  WhenIExecuteTheStepWithTheInput(int input)
    {
        if (this.step is null)
        {
            throw new InvalidOperationException("You must configure the step before execution.");
        }

        SharedSteps.SetOutput(this.scenarioContext, this.step(input));
    }

    private SyncPipelineStep<int> Selector(int state)
    {
        if (this.selector is null)
        {
            throw new InvalidOperationException("You must configure the selector before execution.");
        }

        return this.selector(state);
    }
}