// <copyright file="ChooseOperatorSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using TechTalk.SpecFlow;

namespace Corvus.Pipelines.Specs.Steps;

[Binding]
public class ChooseOperatorSteps(ScenarioContext scenarioContext)
{
    private readonly ScenarioContext scenarioContext = scenarioContext;

    private Func<int, PipelineStep<int>>? selector;
    private PipelineStep<int>? step;

    [Given("I create a selector with the following configuration:")]
    public void GivenICreateASelectorWithTheFollowingConfiguration(Table table)
    {
        this.selector = SharedSteps.BuildSelector<int>(table.Rows.Select(r => (r["Match"], r["Pipeline step"])));
    }

    [When("I produce a step by calling the Choose\\(\\) method with the selector")]
    public void WhenIProduceAStepByCallingTheChooseMethodWithTheSelector()
    {
        this.step = Pipeline.Choose<int>(this.Selector);
    }

    [When("I execute the step with the input (.*)")]
    public async Task WhenIExecuteTheStepWithTheInput(int input)
    {
        if (this.step is null)
        {
            throw new InvalidOperationException("You must configure the step before execution.");
        }

        SharedSteps.SetOutput(this.scenarioContext, await this.step(input).ConfigureAwait(false));
    }

    private PipelineStep<int> Selector(int state)
    {
        if (this.selector is null)
        {
            throw new InvalidOperationException("You must configure the selector before execution.");
        }

        return this.selector(state);
    }
}