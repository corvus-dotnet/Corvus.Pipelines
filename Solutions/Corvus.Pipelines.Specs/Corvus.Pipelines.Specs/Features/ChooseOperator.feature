Feature: Choose Operator

Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for async steps

	Given I create a selector with the following configuration:
		| Match | Pipeline step                     |
		| 1     | state => ValueTask.FromResult(5)  |
		| 2     | state => ValueTask.FromResult(10) |
		| 3     | state => ValueTask.FromResult(15) |
		| _     | state => ValueTask.FromResult(0)  |
	When I produce a step by calling the Choose() method with the selector
	And I execute the step with the input <input>
	Then the output should be <expected_output>

Examples:
	| input | expected_output |
	| 1     | 5               |
	| 2     | 10              |
	| 3     | 15              |
	| 4     | 0               |
	| 0     | 0               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() operator for sync steps

	Given I create a synchronous selector with the following configuration:
		| Match | Pipeline step |
		| 1     | state => 5    |
		| 2     | state => 10   |
		| 3     | state => 15   |
		| _     | state => 0    |
	When I produce a synchronous step by calling the Choose() method with the selector
	And I execute the synchronous step with the input <input>
	Then the output should be <expected_output>

Examples:
	| input | expected_output |
	| 1     | 5               |
	| 2     | 10              |
	| 3     | 15              |
	| 4     | 0               |
	| 0     | 0               |

# WIP:
#Scenario Outline: Test Corvus.Pipelines.Pipeline.Choose() method operating on a preceding step.
#
#	Given I create a selector with the following configuration:
#		| Match | Pipeline step                             |
#		| 1     | state => ValueTask.FromResult(state * 5)  |
#		| 2     | state => ValueTask.FromResult(state * 10) |
#		| 3     | state => ValueTask.FromResult(state * 15) |
#		| _     | state => ValueTask.FromResult(0)          |
#	And I create a root step with the following configuration:
#		| Pipeline step                             |
#		| state => ValueTask.FromResult(state  + 1) |
#	When I produce a step by applying the Choose() operator to the root step with the selector
#	And I execute the step with the input <input>
#	Then the output should be <expected_output>
#
#Examples:
#	| input | expected_output |
#	| 1     | 20              |
#	| 2     | 45              |
#	| 3     | 0               |
#	| 4     | 0               |
#	| 0     | 5               |