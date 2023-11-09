Feature: Build Operator

Scenario Outline: Test Corvus.Pipelines.Pipeline.Build() operator for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                       |
		| Step1     | int        | async         | state => ValueTask.FromResult(state + 1)              |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)              |
		| Step3     | int        | async         | state => ValueTask.FromResult(state + 3)              |
		| TestStep  | int        | async         | Pipeline.Build(Steps.Step1, Steps.Step2, Steps.Step3) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 7               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Build() operator for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                       |
		| Step1     | int        | sync          | state => state + 1                                    |
		| Step2     | int        | sync          | state => state + 2                                    |
		| Step3     | int        | sync          | state => state + 3                                    |
		| TestStep  | int        | sync          | Pipeline.Build(Steps.Step1, Steps.Step2, Steps.Step3) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 7               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Build() operator with termination for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                           |
		| Step1     | int        | async         | state => ValueTask.FromResult(state + 1)                                  |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                  |
		| Step3     | int        | async         | state => ValueTask.FromResult(state + 3)                                  |
		| TestStep  | int        | async         | Pipeline.Build(state => state > 3, Steps.Step1, Steps.Step2, Steps.Step3) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |
	| 3     | 4               |
	| 5     | 5               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Build() operator with termination for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                           |
		| Step1     | int        | sync          | state => state + 1                                                        |
		| Step2     | int        | sync          | state => state + 2                                                        |
		| Step3     | int        | sync          | state => state + 3                                                        |
		| TestStep  | int        | sync          | Pipeline.Build(state => state > 3, Steps.Step1, Steps.Step2, Steps.Step3) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |
	| 3     | 4               |
	| 5     | 5               |