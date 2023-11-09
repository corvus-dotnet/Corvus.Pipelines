Feature: Logging Build Operator

Scenario Outline: Test the logging overloads of Corvus.Pipelines.Pipeline.Build() operator for async steps without explicit names

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                             |
		| Step1     | int        | async         | state => ValueTask.FromResult(state + 1)                                                    |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                                    |
		| Step3     | int        | async         | state => ValueTask.FromResult(state + 3)                                                    |
		| TestStep  | int        | async         | Pipeline.Build("TestPipeline", LogLevel.Information, Steps.Step1, Steps.Step2, Steps.Step3) |
	And I create the service instances
		| Service type | Instance name | Factory method              |
		| ILogger      | TestLogger    | TestLogger.CreateInstance() |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>
	And the log Services.TestLogger should contain the following entries
		| LogLevel    | Message                           |
		| Information | Pipeline "TestPipeline" started   |
		| Information | Step "Step1" started              |
		| Information | Step "Step1" completed            |
		| Information | Step "Step2" started              |
		| Information | Step "Step2" completed            |
		| Information | Step "Step3" started              |
		| Information | Step "Step3" completed            |
		| Information | Pipeline "TestPipeline" completed |

Examples:
	| Input                                          | Expected output           |
	| LoggableInt32State.For(1, Services.TestLogger) | LoggableInt32State.For(7) |