Feature: Test the Catch() operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Catch() operator for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                          |
		| Step1     | int        | async         | state => ValueTask.FromResult(state + 1) |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2) |
		| TestStep  | int        | async         | Steps.Step1.Bind(Steps.Step2)            |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Bind() operator for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition               |
		| Step1     | int        | sync          | state => state + 1            |
		| Step2     | int        | sync          | state => state + 2            |
		| TestStep  | int        | sync          | Steps.Step1.Bind(Steps.Step2) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |