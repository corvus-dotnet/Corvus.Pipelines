Feature: ToAsync Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.ToAsync() operator for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition       |
		| Step1     | int        | sync          | state => state + 1    |
		| TestStep  | int        | async         | Steps.Step1.ToAsync() |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 2               |