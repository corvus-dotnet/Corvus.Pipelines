Feature: OnEntry OnExit and OnEntryAndExit

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnEntry() operators for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                           |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 1)                              |
		| TestStep  | LoggableInt32State | async         | Step1.OnEntry(state => Services.Logger.Log<Log level>("Logged on entry: {state}", state)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message            | Scope |
		| <Log level> | Logged on entry: 1 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnExit() operators for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                     |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 1)                                                        |
		| TestStep  | LoggableInt32State | async         | Step1.OnExit((before, after) => Services.Logger.Log<Log level>("Logged on exit: {before}, {after}", before, after)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message              | Scope |
		| <Log level> | Logged on exit: 1, 2 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnEntryAndExit() operators for async steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                                                         |
		| Step1     | LoggableInt32State | async         | state => ValueTask.FromResult<LoggableInt32State>(state + 1)                                                                                                                                            |
		| TestStep  | LoggableInt32State | async         | Step1.OnEntryAndExit(state => Services.Logger.Log<Log level>("Logged on entry: {state}", state), (before, after) => Services.Logger.Log<Log level>("Logged on exit: {before}, {after}", before, after)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the async step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message              | Scope |
		| <Log level> | Logged on entry: 1   |       |
		| <Log level> | Logged on exit: 1, 2 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnEntry() operators for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                           |
		| Step1     | LoggableInt32State | sync          | state => state + 1                                                                        |
		| TestStep  | LoggableInt32State | sync          | Step1.OnEntry(state => Services.Logger.Log<Log level>("Logged on entry: {state}", state)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message            | Scope |
		| <Log level> | Logged on entry: 1 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnExit() operators for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                     |
		| Step1     | LoggableInt32State | sync          | state => state + 1                                                                                                  |
		| TestStep  | LoggableInt32State | sync          | Step1.OnExit((before, after) => Services.Logger.Log<Log level>("Logged on exit: {before}, {after}", before, after)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message              | Scope |
		| <Log level> | Logged on exit: 1, 2 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |

Scenario Outline: Test the Corvus.Pipelines.PipelineExtensions.OnEntryAndExit() operators for sync steps

	Given I produce the steps
		| Step name | State type         | Sync or async | Step definition                                                                                                                                                                                         |
		| Step1     | LoggableInt32State | sync          | state => state + 1                                                                                                                                                                                      |
		| TestStep  | LoggableInt32State | sync          | Step1.OnEntryAndExit(state => Services.Logger.Log<Log level>("Logged on entry: {state}", state), (before, after) => Services.Logger.Log<Log level>("Logged on exit: {before}, {after}", before, after)) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync step "TestStep" with the input of type "LoggableInt32State" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"
	And the log Services.Logger should contain the following entries
		| Log level   | Message              | Scope |
		| <Log level> | Logged on entry: 1   |       |
		| <Log level> | Logged on exit: 1, 2 |       |
Examples:
	| Input                                      | Expected output           | Log level   |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Debug       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Trace       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Information |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Warning     |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Error       |
	| LoggableInt32State.For(1, Services.Logger) | LoggableInt32State.For(2) | Critical    |
