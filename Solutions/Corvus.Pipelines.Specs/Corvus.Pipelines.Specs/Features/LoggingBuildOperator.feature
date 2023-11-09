Feature: Logging Build Operator

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                              |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 1)                                                                 |
		| Step2     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 2)                                                                 |
		| Step3     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 3)                                                                 |
		| TestStep  | LoggableInt32State | async         | Pipeline.Build("TestPipeline", LogLevel.Information, Steps.Step1.WithName(), Steps.Step2.WithName(), Steps.Step3.WithName()) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be <Expected output>
	And the log Services.Logger should contain the following entries
		| Log level   | Message                           |
		| Information | entered                           |
		| Information | exited                            |
		| Information | entered                           |
		| Information | exited                            |
		| Information | entered                           |
		| Information | exited                            |

Examples:
	| Input                                      | Expected output           |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) |

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                              |
		| Step1     | LoggableInt32State | sync         | state => state + 1                                                                 |
		| Step2     | LoggableInt32State | sync         | state => state + 2                                                                 |
		| Step3     | LoggableInt32State | sync         | state => state + 3                                                                 |
		| TestStep  | LoggableInt32State | sync         | Pipeline.Build("TestPipeline", LogLevel.Information, Steps.Step1.WithName(), Steps.Step2.WithName(), Steps.Step3.WithName()) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be <Expected output>
	And the log Services.Logger should contain the following entries
		| Log level   | Message                           |
		| Information | entered                           |
		| Information | exited                            |
		| Information | entered                           |
		| Information | exited                            |
		| Information | entered                           |
		| Information | exited                            |

Examples:
	| Input                                      | Expected output           |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(7) |