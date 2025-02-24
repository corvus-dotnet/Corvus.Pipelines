Feature: FirstToComplete Operator

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 2 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 2))                                         |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>)                                           |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step |
	| 1     | CancellableInt32State.For(3) | Step1      | Step2       |
	| 1     | CancellableInt32State.For(3) | Step2      | Step1       |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 3 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 2); } |
		| Step3     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                         |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>, Steps.<Third step>)                       |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step | Third step |
	| 1     | CancellableInt32State.For(4) | Step1      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step3       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step1       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step3       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step2       | Step1      |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 5 steps

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                                      |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 1); }                    |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 2); }                    |
		| Step3     | CancellableInt32State | async         | state => ValueTask.FromResult(state.WithValue(state + 3))                                                            |
		| Step4     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 4); }                    |
		| Step5     | CancellableInt32State | async         | async state => { await Task.Delay(20).ConfigureAwait(false); return state.WithValue(state + 5); }                    |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>, Steps.<Third step>, Steps.<Fourth step>, Steps.<Fifth step>) |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step | Third step | Fourth step | Fifth step |
	| 1     | CancellableInt32State.For(4) | Step1      | Step2       | Step3      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step3       | Step4      | Step5       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step4       | Step5      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step5       | Step2      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step1       | Step3      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step3       | Step4      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step4       | Step5      | Step1       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step5       | Step1      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step1       | Step2      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step2       | Step4      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step4       | Step5      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step5       | Step1      | Step2       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step1       | Step2      | Step3       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step2       | Step3      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step3       | Step5      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step5       | Step1      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step1       | Step2      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step2       | Step3      | Step4       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step3       | Step4      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step4       | Step1      | Step2       | Step3      |

	
Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 2 steps fully async

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(10).ConfigureAwait(false); return state.WithValue(state + 2); } |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>)                                           |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step |
	| 1     | CancellableInt32State.For(3) | Step1      | Step2       |
	| 1     | CancellableInt32State.For(3) | Step2      | Step1       |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 3 steps fully async

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                   |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 1); } |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 2); } |
		| Step3     | CancellableInt32State | async         | async state => { await Task.Delay(10).ConfigureAwait(false); return state.WithValue(state + 3); } |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>, Steps.<Third step>)                       |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step | Third step |
	| 1     | CancellableInt32State.For(4) | Step1      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step3       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step1       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step3       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step2       | Step1      |

Scenario Outline: Test Corvus.Pipelines.PipelineExtensions.FirstToComplete() operator for 5 steps fully async

	Given I produce the steps
		| Step name | State type            | Sync or async | Step definition                                                                                                      |
		| Step1     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 1); }                    |
		| Step2     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 2); }                    |
		| Step3     | CancellableInt32State | async         | async state => { await Task.Delay(10).ConfigureAwait(false); return state.WithValue(state + 3); }                    |
		| Step4     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 4); }                    |
		| Step5     | CancellableInt32State | async         | async state => { await Task.Delay(500).ConfigureAwait(false); return state.WithValue(state + 5); }                    |
		| TestStep  | CancellableInt32State | async         | Steps.<First step>.FirstToComplete(Steps.<Second step>, Steps.<Third step>, Steps.<Fourth step>, Steps.<Fifth step>) |
	When I execute the async step "TestStep" with the input of type "CancellableInt32State" <Input>
	Then the async output of "TestStep" should be "<Expected output>"

Examples:
	| Input | Expected output              | First step | Second step | Third step | Fourth step | Fifth step |
	| 1     | CancellableInt32State.For(4) | Step1      | Step2       | Step3      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step3       | Step4      | Step5       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step4       | Step5      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step1      | Step5       | Step2      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step1       | Step3      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step3       | Step4      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step4       | Step5      | Step1       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step2      | Step5       | Step1      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step1       | Step2      | Step4       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step2       | Step4      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step4       | Step5      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step3      | Step5       | Step1      | Step2       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step1       | Step2      | Step3       | Step5      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step2       | Step3      | Step5       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step3       | Step5      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step4      | Step5       | Step1      | Step2       | Step3      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step1       | Step2      | Step3       | Step4      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step2       | Step3      | Step4       | Step1      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step3       | Step4      | Step1       | Step2      |
	| 1     | CancellableInt32State.For(4) | Step5      | Step4       | Step1      | Step2       | Step3      |