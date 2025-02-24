Feature: Current Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Bind() operator for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition         |
		| TestStep  | int        | async         | Pipeline.Current<int>() |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Bind() operator for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition             |
		| TestStep  | int        | sync          | Pipeline.CurrentSync<int>() |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1               |