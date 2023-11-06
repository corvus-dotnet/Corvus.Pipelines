Feature: Bind Operator

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator for async steps

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

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator for sync steps

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

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator with sync wrap and unwrap for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                |
		| Step1     | decimal    | async         | state => ValueTask.FromResult(state + 1m)                                                      |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                                       |
		| BoundStep | int        | async         | Steps.Step1.Bind<decimal, int>((int input) => (decimal)input, (input, output) => (int)output ) |
		| TestStep  | int        | async         | Steps.BoundStep.Bind(Steps.Step2)                                                              |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator with sync wrap and unwrap for sync steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                |
		| Step1     | decimal    | sync          | state => state + 1m                                                                            |
		| Step2     | int        | sync          | state => state + 2                                                                             |
		| BoundStep | int        | sync          | Steps.Step1.Bind<decimal, int>((int input) => (decimal)input, (input, output) => (int)output ) |
		| TestStep  | int        | sync          | Steps.BoundStep.Bind(Steps.Step2)                                                              |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator with sync wrap and async unwrap for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                     |
		| Step1     | decimal    | async         | state => ValueTask.FromResult(state + 1m)                                                                           |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                                                            |
		| BoundStep | int        | async         | Steps.Step1.Bind<decimal, int>((int input) => (decimal)input, (input, output) => ValueTask.FromResult((int)output)) |
		| TestStep  | int        | async         | Steps.BoundStep.Bind(Steps.Step2)                                                                                   |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator with async wrap and sync unwrap for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                      |
		| Step1     | decimal    | async         | state => ValueTask.FromResult(state + 1m)                                                                            |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                                                             |
		| BoundStep | int        | async         | Steps.Step1.Bind<decimal, int>((int input) => ValueTask.FromResult((decimal)input), (input, output) => (int)output ) |
		| TestStep  | int        | async         | Steps.BoundStep.Bind(Steps.Step2)                                                                                    |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |

Scenario Outline: Test Corvus.Pipelines.Pipeline.Bind() operator with async wrap and unwrap for async steps

	Given I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                     |
		| Step1     | decimal    | async         | state => ValueTask.FromResult(state + 1m)                                                                           |
		| Step2     | int        | async         | state => ValueTask.FromResult(state + 2)                                                                            |
		| BoundStep | int        | async         | Steps.Step1.Bind<decimal, int>((int input) => ValueTask.FromResult((decimal)input), (input, output) => ValueTask.FromResult((int)output)) |
		| TestStep  | int        | async         | Steps.BoundStep.Bind(Steps.Step2)                                                                                   |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 4               |