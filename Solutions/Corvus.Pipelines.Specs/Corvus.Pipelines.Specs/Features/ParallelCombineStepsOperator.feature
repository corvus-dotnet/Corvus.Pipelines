Feature: ParallelCombineSteps Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.ParallelCombineSteps() operator for 2 async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                          |
		| Step1     | int        | async         | state => ValueTask.FromResult(state + 1) |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2) |
		| TestStep  | (int, int) | async         | Steps.Step1.ParallelCombineSteps(Steps.Step2)    |
	When I execute the async step "TestStep" with the input of type "(int, int)" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input  | Expected output |
	| (1, 1) | (2, 3)          |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.ParallelCombineSteps() operator for 3 async steps

	Given I produce the steps
		| Step name | State type      | Sync or async | Step definition                                    |
		| Step1     | int             | async         | state => ValueTask.FromResult(state + 1)           |
		| Step2     | int             | async         | state => ValueTask.FromResult(state + 2)           |
		| Step3     | int             | async         | state => ValueTask.FromResult(state + 3)           |
		| TestStep  | (int, int, int) | async         | Steps.Step1.ParallelCombineSteps(Steps.Step2, Steps.Step3) |
	When I execute the async step "TestStep" with the input of type "(int, int, int)" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input     | Expected output |
	| (1, 1, 1) | (2, 3, 4)       |