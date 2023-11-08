Feature: FirstToComplete Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 2 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 2))                                         |
		| TestStep  | CancellableInt32State | async         | Steps.Step1.FirstToComplete(Steps.Step2)                                                          |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output              |
	| 1     | CancellableInt32State.For(3) |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 3 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 2); } |
		| Step3     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                         |
		| TestStep  | CancellableInt32State | async         | Steps.Step1.FirstToComplete(Steps.Step2, Steps.Step3)                                             |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output              |
	| 1     | CancellableInt32State.For(4) |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 5 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 2); } |
		| Step3     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                         |
		| Step4     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 4); } |
		| Step5     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 5); } |
		| TestStep  | CancellableInt32State | async         | Steps.Step1.FirstToComplete(Steps.Step2, Steps.Step3, Steps.Step4, Steps.Step5)                   |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output              |
	| 1     | CancellableInt32State.For(4) |