Feature: Logging Build Operator

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                                                                             |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 1))                                                                                                                                                                   |
		| Step2     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 2))                                                                                                                                                                   |
		| Step3     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                                                                                                                                                   |
		| TestStep  | LoggableInt32State | async         | Pipeline.Build(Steps.Step1.Log(logLevel: LogLevel.<Log level>), Steps.Step2.Log(logLevel: LogLevel.<Log level>), Steps.Step3.Log(logLevel: LogLevel.<Log level>)).Log(logLevel: LogLevel.<Log level>, name: "TestPipeline") |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message | Scope        |
		| <Log level> | entered | TestPipeline |
		| <Log level> | entered | Steps.Step1  |
		| <Log level> | exited  | Steps.Step1  |
		| <Log level> | entered | Steps.Step2  |
		| <Log level> | exited  | Steps.Step2  |
		| <Log level> | entered | Steps.Step3  |
		| <Log level> | exited  | Steps.Step3  |
		| <Log level> | exited  | TestPipeline |


Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Critical    |

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                                                                             |
		| Step1     | LoggableInt32State | sync          | state => state.WithValue(state + 1)                                                                                                                                                                                         |
		| Step2     | LoggableInt32State | sync          | state => state.WithValue(state + 2)                                                                                                                                                                                         |
		| Step3     | LoggableInt32State | sync          | state => state.WithValue(state + 3)                                                                                                                                                                                         |
		| TestStep  | LoggableInt32State | sync          | Pipeline.Build(Steps.Step1.Log(logLevel: LogLevel.<Log level>), Steps.Step2.Log(logLevel: LogLevel.<Log level>), Steps.Step3.Log(logLevel: LogLevel.<Log level>)).Log(logLevel: LogLevel.<Log level>, name: "TestPipeline") |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message | Scope        |
		| <Log level> | entered | TestPipeline |
		| <Log level> | entered | Steps.Step1  |
		| <Log level> | exited  | Steps.Step1  |
		| <Log level> | entered | Steps.Step2  |
		| <Log level> | exited  | Steps.Step2  |
		| <Log level> | entered | Steps.Step3  |
		| <Log level> | exited  | Steps.Step3  |
		| <Log level> | exited  | TestPipeline |

Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) | Critical    |

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator with termination for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                            |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 1))                                                                                                                  |
		| Step2     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 2))                                                                                                                  |
		| Step3     | LoggableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                                                                                                  |
		| TestStep  | LoggableInt32State | async         | Pipeline.Build((LoggableInt32State state) => state > 3, Steps.Step1.Log(), Steps.Step2.Log(), Steps.Step3.Log()).Log(logLevel: LogLevel.<Log level>, name: "TestPipeline") |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(4) | Debug       |
	| LoggableInt32State.For(3, Services.Logger) | LoggableInt32State.For(4) | Debug       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Debug       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Trace       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Information |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Warning     |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Error       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Critical    |

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator with termination for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                            |
		| Step1     | LoggableInt32State | sync          | state => state.WithValue(state + 1)                                                                                                                                        |
		| Step2     | LoggableInt32State | sync          | state => state.WithValue(state + 2)                                                                                                                                        |
		| Step3     | LoggableInt32State | sync          | state => state.WithValue(state + 3)                                                                                                                                        |
		| TestStep  | LoggableInt32State | sync          | Pipeline.Build((LoggableInt32State state) => state > 3, Steps.Step1.Log(), Steps.Step2.Log(), Steps.Step3.Log()).Log(logLevel: LogLevel.<Log level>, name: "TestPipeline") |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(4) | Debug       |
	| LoggableInt32State.For(3, Services.Logger) | LoggableInt32State.For(4) | Debug       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Debug       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Trace       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Information |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Warning     |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Error       |
	| LoggableInt32State.For(5, Services.Logger) | LoggableInt32State.For(5) | Critical    |
