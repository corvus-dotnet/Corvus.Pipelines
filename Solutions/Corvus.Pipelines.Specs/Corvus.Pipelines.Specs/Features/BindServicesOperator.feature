Feature: Bind services

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with async steps

	Given I define the functions
		| Function name    | Function type                        | Function definition                                                     |
		| Service1         | Func<int>                            | () => 10                                                                |
		| StepWithServices | Func<int, Func<int>, ValueTask<int>> | (state, service1) => ValueTask.FromResult(state + (state * service1())) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                             |
		| TestStep  | int        | async         | Functions.StepWithServices.BindServices(Functions.Service1) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 11              |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with sync steps

	Given I define the functions
		| Function name    | Function type             | Function definition                               |
		| Service1         | Func<int>                 | () => 10                                          |
		| StepWithServices | Func<int, Func<int>, int> | (state, service1) => state + (state * service1()) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                             |
		| TestStep  | int        | sync          | Functions.StepWithServices.BindServices(Functions.Service1) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 11              |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with async steps and 2 services

	Given I define the functions
		| Function name    | Function type                                   | Function definition                                                                                      |
		| Service1         | Func<int>                                       | () => 10                                                                                                 |
		| Service2         | Func<int>                                       | () => 100                                                                                                |
		| StepWithServices | Func<int, Func<int>, Func<int>, ValueTask<int>> | (state, service1, service2) => ValueTask.FromResult(state + (state * service1()) + (state * service2())) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                 |
		| TestStep  | int        | async         | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 111             |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with sync steps and 2 services

	Given I define the functions
		| Function name    | Function type                        | Function definition                                                                |
		| Service1         | Func<int>                            | () => 10                                                                           |
		| Service2         | Func<int>                            | () => 100                                                                          |
		| StepWithServices | Func<int, Func<int>, Func<int>, int> | (state, service1, service2) => state + (state * service1()) + (state * service2()) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                 |
		| TestStep  | int        | sync          | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 111             |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with async steps and 3 services

	Given I define the functions
		| Function name    | Function type                                              | Function definition                                                                                                                       |
		| Service1         | Func<int>                                                  | () => 10                                                                                                                                  |
		| Service2         | Func<int>                                                  | () => 100                                                                                                                                 |
		| Service3         | Func<int>                                                  | () => 1_000                                                                                                                               |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, ValueTask<int>> | (state, service1, service2, service3) => ValueTask.FromResult(state + (state * service1()) + (state * service2()) + (state * service3())) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                     |
		| TestStep  | int        | async         | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 1111            |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with sync steps and 3 services

	Given I define the functions
		| Function name    | Function type                                   | Function definition                                                                                                 |
		| Service1         | Func<int>                                       | () => 10                                                                                                            |
		| Service2         | Func<int>                                       | () => 100                                                                                                           |
		| Service3         | Func<int>                                       | () => 1_000                                                                                                         |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, int> | (state, service1, service2, service3) => state + (state * service1()) + (state * service2()) + (state * service3()) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                     |
		| TestStep  | int        | sync          | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 1111            |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with async steps and 4 services

	Given I define the functions
		| Function name    | Function type                                                         | Function definition                                                                                                                                                        |
		| Service1         | Func<int>                                                             | () => 10                                                                                                                                                                   |
		| Service2         | Func<int>                                                             | () => 100                                                                                                                                                                  |
		| Service3         | Func<int>                                                             | () => 1_000                                                                                                                                                                |
		| Service4         | Func<int>                                                             | () => 10_000                                                                                                                                                               |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, Func<int>, ValueTask<int>> | (state, service1, service2, service3, service4) => ValueTask.FromResult(state + (state * service1()) + (state * service2()) + (state * service3()) + (state * service4())) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                         |
		| TestStep  | int        | async         | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3, Functions.Service4) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 11111           |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with sync steps and 4 services

	Given I define the functions
		| Function name    | Function type                                              | Function definition                                                                                                                                  |
		| Service1         | Func<int>                                                  | () => 10                                                                                                                                             |
		| Service2         | Func<int>                                                  | () => 100                                                                                                                                            |
		| Service3         | Func<int>                                                  | () => 1_000                                                                                                                                          |
		| Service4         | Func<int>                                                  | () => 10_000                                                                                                                                         |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, Func<int>, int> | (state, service1, service2, service3, service4) => state + (state * service1()) + (state * service2()) + (state * service3()) + (state * service4()) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                         |
		| TestStep  | int        | sync          | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3, Functions.Service4) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 11111           |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with async steps and 5 services

	Given I define the functions
		| Function name    | Function type                                                                    | Function definition                                                                                                                                                                                         |
		| Service1         | Func<int>                                                                        | () => 10                                                                                                                                                                                                    |
		| Service2         | Func<int>                                                                        | () => 100                                                                                                                                                                                                   |
		| Service3         | Func<int>                                                                        | () => 1_000                                                                                                                                                                                                 |
		| Service4         | Func<int>                                                                        | () => 10_000                                                                                                                                                                                                |
		| Service5         | Func<int>                                                                        | () => 100_000                                                                                                                                                                                               |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, Func<int>, Func<int>, ValueTask<int>> | (state, service1, service2, service3, service4, service5) => ValueTask.FromResult(state + (state * service1()) + (state * service2()) + (state * service3()) + (state * service4()) + (state * service5())) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                                             |
		| TestStep  | int        | async         | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3, Functions.Service4, Functions.Service5) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 111111          |

Scenario Outline: Test Corvus.Pipelines.Pipeline.BindServices() operator with sync steps and 5 services

	Given I define the functions
		| Function name    | Function type                                                         | Function definition                                                                                                                                                                   |
		| Service1         | Func<int>                                                             | () => 10                                                                                                                                                                              |
		| Service2         | Func<int>                                                             | () => 100                                                                                                                                                                             |
		| Service3         | Func<int>                                                             | () => 1_000                                                                                                                                                                           |
		| Service4         | Func<int>                                                             | () => 10_000                                                                                                                                                                          |
		| Service5         | Func<int>                                                             | () => 100_000                                                                                                                                                                         |
		| StepWithServices | Func<int, Func<int>, Func<int>, Func<int>, Func<int>, Func<int>, int> | (state, service1, service2, service3, service4, service5) => state + (state * service1()) + (state * service2()) + (state * service3()) + (state * service4()) + (state * service5()) |
	And I produce the steps
		| Step name | State type | Sync or async | Step definition                                                                                                                             |
		| TestStep  | int        | sync          | Functions.StepWithServices.BindServices(Functions.Service1, Functions.Service2, Functions.Service3, Functions.Service4, Functions.Service5) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be <Expected output>

Examples:
	| Input | Expected output |
	| 1     | 111111          |