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


Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Retry() operator for async steps and both should retry and before retry

	Given I produce the steps
		| Step name   | State type                                                 | Sync or async | Step definition                                                                                                                                                                                                                                 |
		| BeforeRetry | RetryContext<CanFailState<(int Computed, int RetryCount)>> | async         | context => ValueTask.FromResult<RetryContext<CanFailState<(int Computed, int RetryCount)>>>(new (context.State.WithValue((context.State.Value.Computed, context.FailureCount)), default, default))                                              |
		| Step1       | CanFailState<(int Computed, int RetryCount)>               | async         | state => ValueTask.FromResult(state.Value.Computed == 0 && state.ExecutionStatus == PipelineStepStatus.Success ? state.<Failure type>() : CanFailState<(int Computed, int RetryCount)>.For((state.Value.Computed + 1, state.Value.RetryCount))) |
		| TestStep    | CanFailState<(int Computed, int RetryCount)>               | async         | Steps.Step1.Retry(context => context.State.ExecutionStatus == PipelineStepStatus.TransientFailure, BeforeRetry)                                                                                                                                 |
	When I execute the async step "TestStep" with the input of type "CanFailState<(int Computed, int RetryCount)>" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input                                                    | Expected output                                                             | Failure type     |
	| CanFailState<(int Computed, int RetryCount)>.For((0, 0)) | CanFailState<(int Computed, int RetryCount)>.For((1, 1))                    | TransientFailure |
	| CanFailState<(int Computed, int RetryCount)>.For((0, 0)) | CanFailState<(int Computed, int RetryCount)>.For((0, 0)).PermanentFailure() | PermanentFailure |
	| CanFailState<(int Computed, int RetryCount)>.For((1, 0)) | CanFailState<(int Computed, int RetryCount)>.For((2, 0)).Success()          | Success          |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Retry() operator for sync steps and both should retry and before retry

	Given I produce the steps
		| Step name   | State type                                                 | Sync or async | Step definition                                                                                                                                                                                                           |
		| BeforeRetry | RetryContext<CanFailState<(int Computed, int RetryCount)>> | sync          | context => new (context.State.WithValue((context.State.Value.Computed, context.FailureCount)), default, default)                                                                                                          |
		| Step1       | CanFailState<(int Computed, int RetryCount)>               | sync          | state => state.Value.Computed == 0 && state.ExecutionStatus == PipelineStepStatus.Success ? state.<Failure type>() : CanFailState<(int Computed, int RetryCount)>.For((state.Value.Computed + 1, state.Value.RetryCount)) |
		| TestStep    | CanFailState<(int Computed, int RetryCount)>               | sync          | Steps.Step1.Retry(context => context.State.ExecutionStatus == PipelineStepStatus.TransientFailure, BeforeRetry)                                                                                                           |
	When I execute the sync step "TestStep" with the input of type "CanFailState<(int Computed, int RetryCount)>" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input                                                    | Expected output                                                             | Failure type     |
	| CanFailState<(int Computed, int RetryCount)>.For((0, 0)) | CanFailState<(int Computed, int RetryCount)>.For((1, 1))                    | TransientFailure |
	| CanFailState<(int Computed, int RetryCount)>.For((0, 0)) | CanFailState<(int Computed, int RetryCount)>.For((0, 0)).PermanentFailure() | PermanentFailure |
	| CanFailState<(int Computed, int RetryCount)>.For((1, 0)) | CanFailState<(int Computed, int RetryCount)>.For((2, 0)).Success()          | Success          |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Retry() operator for async steps and retry strategies

	Given I produce the steps
		| Step name | State type        | Sync or async | Step definition                                                                                                         |
		| Step1     | CanFailInt32State | async         | async state => { await Task.Delay(<Delay>).ConfigureAwait(false); return state.WithValue(state + 1).<Failure type>(); } |
		| TestStep  | CanFailInt32State | async         | Steps.Step1.Retry(<Retry policy>)                                                                                       |
	When I execute the async step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input                    | Expected output                             | Failure type     | Delay                     | Retry policy                                                                            |
	| CanFailInt32State.For(0) | CanFailInt32State.For(3).TransientFailure() | TransientFailure | TimeSpan.Zero             | Retry.TransientPolicy<CanFailInt32State>().And(Retry.CountPolicy<CanFailInt32State>(3)) |
	| CanFailInt32State.For(0) | CanFailInt32State.For(1).PermanentFailure() | PermanentFailure | TimeSpan.Zero             | Retry.TransientPolicy<CanFailInt32State>()                                              |
	| CanFailInt32State.For(0) | CanFailInt32State.For(1).TransientFailure() | TransientFailure | TimeSpan.FromSeconds(0.2) | Retry.DurationPolicy<CanFailInt32State>(TimeSpan.FromSeconds(0.1))                      |
	| CanFailInt32State.For(0) | CanFailInt32State.For(5).PermanentFailure() | PermanentFailure | TimeSpan.Zero             | Retry.CountPolicy<CanFailInt32State>(5)                                                 |
