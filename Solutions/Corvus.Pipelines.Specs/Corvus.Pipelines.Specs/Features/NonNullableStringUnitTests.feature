Feature: NonNullableString Unit Tests

Scenario Outline: Default non-nullable string is distinct from NonNullableString constructed from String.Empty

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then the NonNullableString TestString <Comparison> <Expected value>

Examples:
	| Constructor parameter      | Comparison       | Expected value             |
	| default(NonNullableString) | should equal     | default(NonNullableString) |
	| default(NonNullableString) | should not equal | String.Empty               |
	| default(NonNullableString) | should not equal | ""                         |
	| String.Empty               | should not equal | default(NonNullableString) |
	| String.Empty               | should equal     | String.Empty               |
	| String.Empty               | should equal     | ""                         |
	| ""                         | should not equal | default(NonNullableString) |
	| ""                         | should equal     | String.Empty               |
	| ""                         | should equal     | ""                         |
	| String.Empty               | !=               | default(NonNullableString) |
	| String.Empty               | ==               | String.Empty               |
	| String.Empty               | ==               | ""                         |
	| ""                         | !=               | default(NonNullableString) |
	| ""                         | ==               | String.Empty               |
	| ""                         | ==               | ""                         |

Scenario Outline: Comparing values

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then the NonNullableString.Value TestString <Comparison> <Expected value>

Examples:
	| Constructor parameter      | Comparison   | Expected value             |
	| default(NonNullableString) | should equal | default(NonNullableString) |
	| default(NonNullableString) | should equal | String.Empty               |
	| default(NonNullableString) | should equal | ""                         |
	| String.Empty               | should equal | default(NonNullableString) |
	| String.Empty               | should equal | String.Empty               |
	| String.Empty               | should equal | ""                         |
	| ""                         | should equal | default(NonNullableString) |
	| ""                         | should equal | String.Empty               |
	| ""                         | should equal | ""                         |
	| String.Empty               | ==           | default(NonNullableString) |
	| String.Empty               | ==           | String.Empty               |
	| String.Empty               | ==           | ""                         |
	| ""                         | ==           | default(NonNullableString) |
	| ""                         | ==           | String.Empty               |
	| ""                         | ==           | ""                         |
	| abc                        | <=           | Abc                        |
	| Abc                        | <=           | Abc                        |
	| Abc                        | <            | Bbc                        |
	| Bbc                        | >            | Abc                        |
	| Bbc                        | >=           | Bbc                        |

Scenario Outline: Comparing values ignore case

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then the NonNullableString.Value TestString <Comparison> <Expected value> ignoring case

Examples:
	| Constructor parameter      | Comparison   | Expected value             |
	| default(NonNullableString) | should equal | default(NonNullableString) |
	| default(NonNullableString) | should equal | String.Empty               |
	| default(NonNullableString) | should equal | ""                         |
	| String.Empty               | should equal | default(NonNullableString) |
	| String.Empty               | should equal | String.Empty               |
	| String.Empty               | should equal | ""                         |
	| ""                         | should equal | default(NonNullableString) |
	| ""                         | should equal | String.Empty               |
	| ""                         | should equal | ""                         |
	| String.Empty               | ==           | default(NonNullableString) |
	| String.Empty               | ==           | String.Empty               |
	| String.Empty               | ==           | ""                         |
	| ""                         | ==           | default(NonNullableString) |
	| ""                         | ==           | String.Empty               |
	| ""                         | ==           | ""                         |
	| abc                        | <=           | ABC                        |
	| ABC                        | <=           | Abc            b           |
	| Abc                        | <            | bbc                        |
	| bbc                        | >            | aBC                        |
	| bbc                        | >=           | Bbc                        |
	| bbc                        | >=           | ABC                        |
