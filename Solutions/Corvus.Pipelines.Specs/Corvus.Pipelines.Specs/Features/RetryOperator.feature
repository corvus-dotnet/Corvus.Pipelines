Feature: Retry Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Retry() operator for async steps and should retry

	Given I produce the steps
		| Step name | State type        | Sync or async | Step definition                                                                                                                                              |
		| Step1     | CanFailInt32State | async         | state => ValueTask.FromResult(state == 0 && state.ExecutionStatus == PipelineStepStatus.Success ? state.<Failure type>() : CanFailInt32State.For(state + 1)) |
		| TestStep  | CanFailInt32State | async         | Steps.Step1.Retry(context => context.State.ExecutionStatus == PipelineStepStatus.TransientFailure)                                                           |
	When I execute the async step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input                    | Expected output                             | Failure type     |
	| CanFailInt32State.For(0) | CanFailInt32State.For(1)                    | TransientFailure |
	| CanFailInt32State.For(0) | CanFailInt32State.For(0).PermanentFailure() | PermanentFailure |
	| CanFailInt32State.For(1) | CanFailInt32State.For(2).Success()          | Success          |


Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Retry() operator for sync steps and should retry

	Given I produce the steps
		| Step name | State type        | Sync or async | Step definition                                                                                                                        |
		| Step1     | CanFailInt32State | sync          | state => state == 0 && state.ExecutionStatus == PipelineStepStatus.Success ? state.<Failure type>() : CanFailInt32State.For(state + 1) |
		| TestStep  | CanFailInt32State | sync          | Steps.Step1.Retry(context => context.State.ExecutionStatus == PipelineStepStatus.TransientFailure)                                     |
	When I execute the sync step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input                    | Expected output                             | Failure type     |
	| CanFailInt32State.For(0) | CanFailInt32State.For(1)                    | TransientFailure |
	| CanFailInt32State.For(0) | CanFailInt32State.For(0).PermanentFailure() | PermanentFailure |
	| CanFailInt32State.For(1) | CanFailInt32State.For(2).Success()          | Success          |
