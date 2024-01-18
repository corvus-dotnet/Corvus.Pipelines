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

Scenario Outline: Comparing

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then the NonNullableString TestString <Comparison> <Expected value>

Examples:
	| Constructor parameter | Comparison      | Expected value             |
	| String.Empty          | !=              | default(NonNullableString) |
	| String.Empty          | ==              | String.Empty               |
	| String.Empty          | ==              | ""                         |
	| ""                    | !=              | default(NonNullableString) |
	| ""                    | ==              | String.Empty               |
	| ""                    | ==              | ""                         |
	| abc                   | <=              | Abc                        |
	| Abc                   | <=              | Abc                        |
	| Abc                   | <               | Bbc                        |
	| Bbc                   | >               | Abc                        |
	| Bbc                   | >=              | Bbc                        |
	| Bbc                   | ObjectEquals    | Bbc                        |
	| Bbc                   | ObjectEquals    | "Bbc"                      |
	| Bbc                   | ObjectNotEquals | Abc                        |
	| Bbc                   | ObjectNotEquals | "Abc"                      |
	| Bbc                   | ObjectNotEquals | null                       |

Scenario Outline: HashCode

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then the NonNullableString TestString <Comparison> <Expected value>

Examples:
	| Constructor parameter | Comparison        | Expected value             |
	| String.Empty          | NotEqualsHashCode | default(NonNullableString) |
	| String.Empty          | EqualsHashCode    | String.Empty               |
	| String.Empty          | EqualsHashCode    | ""                         |
	| ""                    | NotEqualsHashCode | default(NonNullableString) |
	| ""                    | EqualsHashCode    | String.Empty               |
	| ""                    | EqualsHashCode    | ""                         |
	| abc                   | EqualsHashCode    | abc                        |
	| abc                   | EqualsHashCode    | "abc"                      |
	| abc                   | NotEqualsHashCode | Abc                        |
	| abc                   | NotEqualsHashCode | "Abc"                      |

Scenario Outline: HashCode ignore case

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then ignoring case the NonNullableString TestString <Comparison> <Expected value>

Examples:
	| Constructor parameter | Comparison        | Expected value             |
	| String.Empty          | NotEqualsHashCode | default(NonNullableString) |
	| String.Empty          | EqualsHashCode    | String.Empty               |
	| String.Empty          | EqualsHashCode    | ""                         |
	| ""                    | NotEqualsHashCode | default(NonNullableString) |
	| ""                    | EqualsHashCode    | String.Empty               |
	| ""                    | EqualsHashCode    | ""                         |
	| abc                   | EqualsHashCode    | Abc                        |
	| abc                   | EqualsHashCode    | "Abc"                      |

Scenario Outline: Comparing ignore case

	Given I create the non-nullable strings
		| Name       | Constructor parameter   |
		| TestString | <Constructor parameter> |
	Then ignoring case the NonNullableString TestString <Comparison> <Expected value>

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
	| String.Empty               | ==               | String.Empty               |
	| String.Empty               | ==               | ""                         |
	| ""                         | ==               | String.Empty               |
	| ""                         | ==               | ""                         |
	| abc                        | <=               | ABC                        |
	| ABC                        | <=               | Abc            b           |
	| Abc                        | <                | bbc                        |
	| bbc                        | >                | aBC                        |
	| bbc                        | >=               | Bbc                        |
	| bbc                        | >=               | ABC                        |
