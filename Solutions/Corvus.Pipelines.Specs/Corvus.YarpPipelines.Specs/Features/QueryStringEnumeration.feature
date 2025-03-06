Feature: QueryStringEnumeration

As a pipeline developer
I want to be able to enumerate over the key value pairs in a query string

Scenario Outline: Single entry
    Given the query string '<Query>'
    When I enumerate the query string
    Then the enumerated query string results should be
    | Name   | Value   |
    | <Name> | <Value> |

    Examples:
    | Query                                     | Name                  | Value              |
    | ?param=value                              | param                 | value              |
    | ?param                                    | param                 |                    |
    | ?param=                                   | param                 |                    |
    | ?=value                                   |                       | value              |
    | ?=                                        |                       |                    |
    | ?                                         |                       |                    |
    | ?param%26withampersand=value%3Dwithequals | param%26withampersand | value%3Dwithequals |
    | ?param=value=with=equals                  | param                 | value=with=equals  |

Scenario Outline: Two entries
    Given the query string '<Query>'
    When I enumerate the query string
    Then the enumerated query string results should be
    | Name    | Value    |
    | <Name1> | <Value1> |
    | <Name2> | <Value2> |

    Examples:
    | Query                                    | Name1  | Value1             | Name2  | Value2 |
    | ?param1=value1&param2=value2             | param1 | value1             | param2 | value2 |
    | ?param1=&param2=value2                   | param1 |                    | param2 | value2 |
    | ?param1&param2=value2                    | param1 |                    | param2 | value2 |
    | ?=value1&param2=value2                   |        | value1             | param2 | value2 |
    | ?param1=value1=with=equals&param2=value2 | param1 | value1=with=equals | param2 | value2 |
    | ?&param2=value2                          |        |                    | param2 | value2 |
    | ?=&param2=value2                         |        |                    | param2 | value2 |
    | ?param1=value1&param2=                   | param1 | value1             | param2 |        |
    | ?param1=value1&param2                    | param1 | value1             | param2 |        |
    | ?param1=value1&=value2                   | param1 | value1             |        | value2 |
    | ?param1=value1&=                         | param1 | value1             |        |        |
    | ?param1=value1&                          | param1 | value1             |        |        |


Scenario Outline: Three entries
    Given the query string '<Query>'
    When I enumerate the query string
    Then the enumerated query string results should be
    | Name    | Value    |
    | <Name1> | <Value1> |
    | <Name2> | <Value2> |
    | <Name3> | <Value3> |

    Examples:
    | Query                                      | Name1  | Value1 | Name2  | Value2 | Name3  | Value3 |
    | ?param1=value1&param2=value2&param3=value3 | param1 | value1 | param2 | value2 | param3 | value3 |
    | ?param1=value1&param2=&param3=value3       | param1 | value1 | param2 |        | param3 | value3 |
    | ?param1=value1&param2&param3=value3        | param1 | value1 | param2 |        | param3 | value3 |
    | ?param1=value1&=value2&param3=value3       | param1 | value1 |        | value2 | param3 | value3 |
    | ?param1=value1&=&param3=value3             | param1 | value1 |        |        | param3 | value3 |
    | ?param1=value1&&param3=value3              | param1 | value1 |        |        | param3 | value3 |
