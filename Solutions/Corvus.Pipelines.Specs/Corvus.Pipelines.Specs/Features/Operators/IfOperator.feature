Feature: If Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.If() operator for async steps when then and else

	Given I define the functions
		| Function name | Function type  | Function definition |
		| IfThenElse    | Predicate<int> | state => state == 0 |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                       |
		| ThenStep  | int        | async         | state => ValueTask.FromResult(state + 1)              |
		| ElseStep  | int        | async         | state => ValueTask.FromResult(state + 2)              |
		| TestStep  | int        | async         | Pipeline.If(Functions.IfThenElse, ThenStep, ElseStep) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 0     | 1               |
	| 1     | 3               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.If() operator for async steps with then

	Given I define the functions
		| Function name | Function type  | Function definition |
		| IfThenElse    | Predicate<int> | state => state == 0 |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                   |
		| ThenStep  | int        | async         | state => ValueTask.FromResult(state + 1)          |
		| TestStep  | int        | async         | Pipeline.If(Functions.IfThenElse, Steps.ThenStep) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 0     | 1               |
	| 2     | 2               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.If() operator for sync steps when then and else

	Given I define the functions
		| Function name | Function type  | Function definition |
		| IfThenElse    | Predicate<int> | state => state == 0 |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                       |
		| ThenStep  | int        | sync          | state => state + 1                                    |
		| ElseStep  | int        | sync          | state => state + 2                                    |
		| TestStep  | int        | sync          | Pipeline.If(Functions.IfThenElse, ThenStep, ElseStep) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 0     | 1               |
	| 1     | 3               |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.If() operator for sync steps with then

	Given I define the functions
		| Function name | Function type  | Function definition |
		| IfThenElse    | Predicate<int> | state => state == 0 |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                   |
		| ThenStep  | int        | sync          | state => state + 1                                |
		| TestStep  | int        | sync          | Pipeline.If(Functions.IfThenElse, Steps.ThenStep) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 0     | 1               |
	| 2     | 2               |