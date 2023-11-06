Feature: Choose Operator

Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for async steps

	Given I create an async match selector called "TestSelector" for state of type "int" with the following configuration
		| Match | Step definition                   |
		| 1     | state => ValueTask.FromResult(5)  |
		| 2     | state => ValueTask.FromResult(10) |
		| 3     | state => ValueTask.FromResult(15) |
		| _     | state => ValueTask.FromResult(0)  |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                         |
		| TestStep  | int        | async         | Pipeline.Choose(Selectors.TestSelector) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 5               |
	| 2     | 10              |
	| 3     | 15              |
	| 4     | 0               |
	| 0     | 0               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for sync steps

	Given I create a sync match selector called "TestSelector" for state of type "int" with the following configuration
		| Match | Step definition |
		| 1     | state => 5      |
		| 2     | state => 10     |
		| 3     | state => 15     |
		| _     | state => 0      |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                         |
		| TestStep  | int        | sync          | Pipeline.Choose(Selectors.TestSelector) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 5               |
	| 2     | 10              |
	| 3     | 15              |
	| 4     | 0               |
	| 0     | 0               |

	
Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for async steps operating on a preceding step.

	Given I create an async match selector called "TestSelector" for state of type "int" with the following configuration
		| Match | Step definition                           |
		| 1     | state => ValueTask.FromResult(state * 5)  |
		| 2     | state => ValueTask.FromResult(state * 10) |
		| 3     | state => ValueTask.FromResult(state * 15) |
		| _     | state => ValueTask.FromResult(0)          |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                               |
		| RootStep  | int        | async          | state => ValueTask.FromResult(state + 1)      |
		| TestStep  | int        | async         | Steps.RootStep.Choose(Selectors.TestSelector) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 20              |
	| 2     | 45              |
	| 3     | 0               |
	| 4     | 0               |
	| 0     | 5               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for sync steps operating on a preceding step.

	Given I create a sync match selector called "TestSelector" for state of type "int" with the following configuration
		| Match | Step definition     |
		| 1     | state => state * 5  |
		| 2     | state => state * 10 |
		| 3     | state => state * 15 |
		| _     | state => 0          |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                               |
		| RootStep  | int        | sync          | state => state + 1                            |
		| TestStep  | int        | sync          | Steps.RootStep.Choose(Selectors.TestSelector) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 20              |
	| 2     | 45              |
	| 3     | 0               |
	| 4     | 0               |
	| 0     | 5               |

