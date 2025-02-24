Feature: Not logical combinator

Scenario Outline: Normal operation
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                               |
		| Handler1     | int        | bool        | sync          | state => state.Handled(state.Input >= 42)        |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Not(Handlers.Handler1) |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" <Input>
	Then the sync output of "TestHandler" should be handled with result '<Expected output>'

Examples:
	| Input                          | Expected output |
	| HandlerState<int,bool>.For(42) | false           |
	| HandlerState<int,bool>.For(41) | true            |

Scenario: Predicate does not call Handled
	Given I produce the handlers
		| Handler name | Input type | Output type | Sync or async | Handler definition                                                                                                                                                                         |
		| Handler1     | int        | bool        | sync          | state => state                                                                                                                                                                             |
		| TestHandler  | int        | bool        | sync          | LogicalCombinatorHandlers.Not(Handlers.Handler1).Catch((HandlerState<int, bool> state, InvalidOperationException x) => { state.Logger.LogError(x.Message); return state.Handled(false); }) |
	And I create the service instances
		| Service type | Instance name | Factory method   |
		| TestLogger   | Logger        | new TestLogger() |
	When I execute the sync handler "TestHandler" with the input of type "HandlerState<int, bool>" HandlerState<int,bool>.For(42, Services.Logger)
	Then the sync output of "TestHandler" should be handled with result 'false'
	Then the log Services.Logger should contain the following entries at level Warning or above
		| Log level | Message                           | Scope |
		| Error     | Matcher did not handle the state. |       |
