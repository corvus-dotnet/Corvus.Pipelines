Feature: Catch operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Catch() operator for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                   |
		| Step1     | int        | async         | state => state == 0 ? throw new InvalidOperationException() : ValueTask.FromResult(state)         |
		| TestStep  | int        | async         | Steps.Step1.Catch<int, InvalidOperationException>((state, exception) => ValueTask.FromResult(10)) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 0     | 10              |
	| 1     | 1               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Catch() operator for async steps with sync catch

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                           |
		| Step1     | int        | async         | state => state == 0 ? throw new InvalidOperationException() : ValueTask.FromResult(state) |
		| TestStep  | int        | async         | Steps.Step1.Catch<int, InvalidOperationException>((state, exception) => 10)               |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 0     | 10              |
	| 1     | 1               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.Bind() operator for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                             |
		| Step1     | int        | sync          | state => state == 0 ? throw new InvalidOperationException() : state         |
		| TestStep  | int        | sync          | Steps.Step1.Catch<int, InvalidOperationException>((state, exception) => 10) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 0     | 10              |
	| 1     | 1               |
