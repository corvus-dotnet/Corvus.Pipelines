Feature: OnError Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.OnError() operator for async steps

	Given I produce the steps
		| Step name    | State type        | Sync or async | Step definition                                                            |
		| Step1        | CanFailInt32State | async         | state => ValueTask.FromResult(state == 0 ? state.<Failure type>() : state) |
		| ErrorHandler | CanFailInt32State | async         | state => ValueTask.FromResult(state.WithValue(10))                         |
		| TestStep     | CanFailInt32State | async         | Steps.Step1.OnError(ErrorHandler)                                          |
	When I execute the async step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input                    | Expected output                              | Failure type     |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).TransientFailure() | TransientFailure |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).PermanentFailure() | PermanentFailure |
	| CanFailInt32State.For(1) | CanFailInt32State.For(1).Success()           | Success          |


Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.OnError() operator for async steps with a sync handler

	Given I produce the steps
		| Step name    | State type        | Sync or async | Step definition                                                            |
		| Step1        | CanFailInt32State | async         | state => ValueTask.FromResult(state == 0 ? state.<Failure type>() : state) |
		| ErrorHandler | CanFailInt32State | sync          | state => state.WithValue(10)                                               |
		| TestStep     | CanFailInt32State | async         | Steps.Step1.OnError(ErrorHandler)                                          |
	When I execute the async step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input                    | Expected output                              | Failure type     |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).TransientFailure() | TransientFailure |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).PermanentFailure() | PermanentFailure |
	| CanFailInt32State.For(1) | CanFailInt32State.For(1).Success()           | Success          |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.OnError() operator for sync steps

	Given I produce the steps
		| Step name    | State type        | Sync or async | Step definition                                      |
		| Step1        | CanFailInt32State | sync          | state => state == 0 ? state.<Failure type>() : state |
		| ErrorHandler | CanFailInt32State | sync          | state => state.WithValue(10)                         |
		| TestStep     | CanFailInt32State | sync          | Steps.Step1.OnError(ErrorHandler)                    |
	When I execute the sync step "TestStep" with the input of type "CanFailInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input                    | Expected output                              | Failure type     |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).TransientFailure() | TransientFailure |
	| CanFailInt32State.For(0) | CanFailInt32State.For(10).PermanentFailure() | PermanentFailure |
	| CanFailInt32State.For(1) | CanFailInt32State.For(1).Success()           | Success          |