Feature: Or logical combinator

Scenario Outline: One step
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                                                                            |
		| Handler1     | int        | bool        | sync          | state => state.Handled(state.Input >= 42)                                                     |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Or(LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler1)) |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" <Input>
	Then the sync output of "TestHandler" should be handled with result '<Expected output>'

Examples:
	| Input                          | Expected output |
	| HandlerState<int,bool>.For(42) | true            |
	| HandlerState<int,bool>.For(41) | false           |


Scenario: No steps
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                  |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Or<int>() |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" HandlerState<int,bool>.For(42)
	Then the sync output of "TestHandler" should be handled with result 'false'


Scenario Outline: Two step
	Given I produce the handlers
		| Handler name | Input type   | Output type | Sync or async | Handler definition                                                                                                                                             |
		| Handler1     | (bool, bool) | bool        | sync          | state => state.Handled(state.Input.Item1)                                                                                                                      |
		| Handler2     | (bool, bool) | bool        | sync          | state => state.Handled(state.Input.Item2)                                                                                                                      |
		| TestHandler  | (bool, bool) | bool        | sync          | LogicalCombinatorHandlers.Or(LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler1), LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler2)) |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<(bool, bool), bool>" <Input>
	Then the sync output of "TestHandler" should be handled with result '<Expected output>'

Examples:
	| Input                                                | Expected output |
	| HandlerState<(bool, bool), bool>.For((false, false)) | false           |
	| HandlerState<(bool, bool), bool>.For((false, true))  | true            |
	| HandlerState<(bool, bool), bool>.For((true, false))  | true            |
	| HandlerState<(bool, bool), bool>.For((true, true))   | true            |


Scenario: Short circuiting
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                                                                                                                                                                                                                                                                                                       |
		| Handler1     | int        | bool        | sync          | state => state.Handled(true)                                                                                                                                                                                                                                                                                             |
		| Handler2     | int        | bool        | sync          | state => state                                                                                                                                                                                                                                                                                                           |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Or(LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler1).Log(logLevel: LogLevel.Warning, name: "Handler1"), LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler2).Log(logLevel: LogLevel.Warning, name: "Handler2")).Log(logLevel: LogLevel.Warning, name: "TestPipeline") |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" HandlerState<int,bool>.For(42, Services.Logger)
	Then the sync output of "TestHandler" should be handled with result 'true'
	And the log Services.Logger should contain the following entries at level Warning or above
		| Log level | Message | Scope        |
		| Warning   | entered | TestPipeline |
		| Warning   | entered | Handler1     |
		| Warning   | exited  | Handler1     |
		| Warning   | exited  | TestPipeline |


Scenario: Predicate does not call Handled
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                                                                                                                                                                                                                      |
		| Handler1     | int        | bool        | sync          | state => state                                                                                                                                                                                                                          |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Or(LogicalCombinatorHandlers.BindPredicateForOr(Handlers.Handler1)).Catch((HandlerState<int, bool> state, InvalidOperationException x) => { state.Logger.LogError(x.Message); return state.Handled(false); }) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" HandlerState<int,bool>.For(42, Services.Logger)
	Then the sync output of "TestHandler" should be handled with result 'false'
	Then the log Services.Logger should contain the following entries at level Warning or above
		| Log level | Message                           | Scope |
		| Error     | Matcher did not handle the state. |       |
