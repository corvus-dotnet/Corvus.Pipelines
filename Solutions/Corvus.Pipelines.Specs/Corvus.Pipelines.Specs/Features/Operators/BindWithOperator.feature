Feature: BindWith operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps

	Given I produce the steps
		| Step name | State type                                   | Sync or async | Step definition                                                             |
		| Step1     | (int Number, NonNullableString StringNumber) | sync          | state => (state.Number + int.Parse(state.StringNumber), state.StringNumber) |
		| TestStep  | int                                          | sync          | Steps.Step1.BindWith((NonNullableString state) => "32")                     |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 33              |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps

	Given I produce the steps
		| Step name | State type                                   | Sync or async | Step definition                                                                                   |
		| Step1     | (int Number, NonNullableString StringNumber) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.StringNumber), state.StringNumber)) |
		| TestStep  | int                                          | async         | Steps.Step1.BindWith((NonNullableString state) => ValueTask.FromResult<NonNullableString>("32"))  |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 33              |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps and two parameters

	Given I produce the steps
		| Step name | State type                                                 | Sync or async | Step definition                                                                             |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2) | sync          | state => (state.Number + int.Parse(state.SN1) + int.Parse(state.SN2), state.SN1, state.SN2) |
		| TestStep  | int                                                        | sync          | Steps.Step1.BindWith((NonNullableString state) => "32", (NonNullableString state) => "100") |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 133             |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps and two parameters

	Given I produce the steps
		| Step name | State type                                                 | Sync or async | Step definition                                                                                                                                                               |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.SN1) + int.Parse(state.SN2), state.SN1, state.SN2))                                                             |
		| TestStep  | int                                                        | async         | Steps.Step1.BindWith((NonNullableString state) => ValueTask.FromResult<NonNullableString>("32"), (NonNullableString state) => ValueTask.FromResult<NonNullableString>("100")) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 133             |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps and three parameters

	Given I produce the steps
		| Step name | State type                                                                        | Sync or async | Step definition                                                                                                                  |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2, NonNullableString SN3) | sync          | state => (state.Number + int.Parse(state.SN1) + int.Parse(state.SN2) + int.Parse(state.SN3), state.SN1, state.SN2, state.SN3)    |
		| TestStep  | int                                                                               | sync          | Steps.Step1.BindWith((NonNullableString state) => "32", (NonNullableString state) => "100", (NonNullableString state) => "1000") |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1133            |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps and three parameters

	Given I produce the steps
		| Step name | State type                                                                        | Sync or async | Step definition                                                                                                                                                                                                                                             |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2, NonNullableString SN3) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.SN1) + int.Parse(state.SN2)+ int.Parse(state.SN3), state.SN1, state.SN2, state.SN3))                                                                                                          |
		| TestStep  | int                                                                               | async         | Steps.Step1.BindWith((NonNullableString state) => ValueTask.FromResult<NonNullableString>("32"), (NonNullableString state) => ValueTask.FromResult<NonNullableString>("100"), (NonNullableString state) => ValueTask.FromResult<NonNullableString>("1000")) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1133            |


Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition              |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString() |
	And I produce the steps
		| Step name | State type                                   | Sync or async | Step definition                                                             |
		| Step1     | (int Number, NonNullableString StringNumber) | sync          | state => (state.Number + int.Parse(state.StringNumber), state.StringNumber) |
		| TestStep  | int                                          | sync          | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => state)   |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 11              |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition              |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString() |
	And I produce the steps
		| Step name | State type                                   | Sync or async | Step definition                                                                                   |
		| Step1     | (int Number, NonNullableString StringNumber) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.StringNumber), state.StringNumber)) |
		| TestStep  | int                                          | async         | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => ValueTask.FromResult(state))   |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 11              |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps, two parameters, and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition               |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString()  |
		| Seed2         | Func<int, NonNullableString> | state => (state * 100).ToString() |
	And I produce the steps
		| Step name | State type                                                 | Sync or async | Step definition                                                                                                                |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2) | sync          | state => (state.Number + int.Parse(state.SN1) + int.Parse(state.SN2), state.SN1, state.SN2)                                    |
		| TestStep  | int                                                        | sync          | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => state, Functions.Seed2, (NonNullableString state) => state) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 111             |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps, two parameters, and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition               |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString()  |
		| Seed2         | Func<int, NonNullableString> | state => (state * 100).ToString() |
	And I produce the steps
		| Step name | State type                                                 | Sync or async | Step definition                                                                                                                                                            |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.SN1) + int.Parse(state.SN2), state.SN1, state.SN2))                                                          |
		| TestStep  | int                                                        | async         | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => ValueTask.FromResult(state), Functions.Seed2, (NonNullableString state) => ValueTask.FromResult(state)) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 111             |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with sync steps, three parameters, and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition                |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString()   |
		| Seed2         | Func<int, NonNullableString> | state => (state * 100).ToString()  |
		| Seed3         | Func<int, NonNullableString> | state => (state * 1000).ToString() |
	And I produce the steps
		| Step name | State type                                                                        | Sync or async | Step definition                                                                                                                                                                     |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2, NonNullableString SN3) | sync          | state => (state.Number + int.Parse(state.SN1) + int.Parse(state.SN2) + int.Parse(state.SN3), state.SN1, state.SN2, state.SN3)                                                       |
		| TestStep  | int                                                                               | sync          | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => state, Functions.Seed2, (NonNullableString state) => state, Functions.Seed3, (NonNullableString state) => state) |
	When I execute the sync step "TestStep" with the input of type "int" <Input>
	Then the sync output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1111            |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.BindWith() operator with async steps, three parameters, and seed functions

	Given I define the functions
		| Function name | Function type                | Function definition                |
		| Seed1         | Func<int, NonNullableString> | state => (state * 10).ToString()   |
		| Seed2         | Func<int, NonNullableString> | state => (state * 100).ToString()  |
		| Seed3         | Func<int, NonNullableString> | state => (state * 1000).ToString() |
	And I produce the steps
		| Step name | State type                                                                        | Sync or async | Step definition                                                                                                                                                                                                                                       |
		| Step1     | (int Number, NonNullableString SN1, NonNullableString SN2, NonNullableString SN3) | async         | state => ValueTask.FromResult((state.Number + int.Parse(state.SN1) + int.Parse(state.SN2)+ int.Parse(state.SN3), state.SN1, state.SN2, state.SN3))                                                                                                    |
		| TestStep  | int                                                                               | async         | Steps.Step1.BindWith(Functions.Seed1, (NonNullableString state) => ValueTask.FromResult(state), Functions.Seed2, (NonNullableString state) => ValueTask.FromResult(state), Functions.Seed3, (NonNullableString state) => ValueTask.FromResult(state)) |
	When I execute the async step "TestStep" with the input of type "int" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output |
	| 1     | 1111            |