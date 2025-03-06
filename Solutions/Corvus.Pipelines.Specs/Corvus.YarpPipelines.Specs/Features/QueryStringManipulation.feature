Feature: QueryStringManipulation

As a pipeline developer
I want to be able to modify the query string of a URL

Scenario Outline: Remove a single element at the start
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

    Examples:
    | querystringin                                           |
    | ?toremove=value&param2=value2&param3=value3             |
    | ?toremove=value=with=equals&param2=value2&param3=value3 |

Scenario Outline: Remove a single name-only element at the start
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

    Examples:
    | querystringin                          |
    | ?toremove=&param2=value2&param3=value3 |
    | ?toremove&param2=value2&param3=value3  |

Scenario Outline: Remove a single no-name, no-value element at the start
    Given the query string '<querystringin>'
    When I remove '' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

    Examples:
    | querystringin                  |
    | ?&param2=value2&param3=value3  |
    | ?=&param2=value2&param3=value3 |

Scenario: Remove a single no-name, with-value element at the start
    Given the query string '?=value1&param2=value2&param3=value3'
    When I remove '' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

Scenario Outline: Remove a single element in the middle
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<result>'

    Examples:
    | querystringin                               | result                       |
    | ?param1=value1&toremove=value&param3=value3 | ?param1=value1&param3=value3 |
    | ?param1=&toremove=value&param3=value3       | ?param1=&param3=value3       |
    | ?param1&toremove=value&param3=value3        | ?param1&param3=value3        |
    | ?=value1&toremove=value&param3=value3       | ?=value1&param3=value3       |
    | ?=&toremove=value&param3=value3             | ?=&param3=value3             |
    | ?&toremove=value&param3=value3              | ?&param3=value3              |
    | ?param1=value1&toremove=value&param3=value3 | ?param1=value1&param3=value3 |
    | ?param1=value1&toremove=value&param3=       | ?param1=value1&param3=       |
    | ?param1=value1&toremove=value&param3        | ?param1=value1&param3        |
    | ?param1=value1&toremove=value&=value3       | ?param1=value1&=value3       |
    | ?param1=value1&toremove=value&=             | ?param1=value1&=             |
    | ?param1=value1&toremove=value&              | ?param1=value1&              |

Scenario Outline: Remove a single name-only element in the middle
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

    Examples:
    | querystringin                          |
    | ?param1=value1&toremove=&param3=value3 |
    | ?param1=value1&toremove&param3=value3 |

Scenario Outline: Remove a single no-name, no-value element in the middle
    Given the query string '<querystringin>'
    When I remove '' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

    Examples:
    | querystringin                  |
    | ?param1=value1&&param3=value3  |
    | ?param1=value1&=&param3=value3 |

Scenario: Remove a single no-name, with-value element in the middle
    Given the query string '?param1=value1&=value&param3=value3'
    When I remove '' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

Scenario: Remove a single element at the end
    Given the query string '?param1=value1&param2=value2&toremove=value'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param2=value2'

Scenario Outline: Remove a single name-only element at the end
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param2=value2'

    Examples:
    | querystringin                          |
    | ?param1=value1&param2=value2&toremove= |
    | ?param1=value1&param2=value2&toremove  |

Scenario Outline: Remove a single no-name, no-value element at the end
    Given the query string '<querystringin>'
    When I remove '' from the query string
    Then the modified query string should be '?param1=value1&param2=value2'

    Examples:
    | querystringin                  |
    | ?param1=value1&param2=value2&= |
    | ?param1=value1&param2=value2&  |

Scenario: Remove a single no-name, with-value element at the end
    Given the query string '?param1=value1&param3=value3&=value'
    When I remove '' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

Scenario Outline: Remove the only element
    Given the query string '<querystringin>'
    When I remove '<remove>' from the query string
    Then the modified query string should be ''

    Examples:
    | querystringin   | remove   |
    | ?toremove=value | toremove |
    | ?toremove=      | toremove |
    | ?toremove       | toremove |
    | ?=value         |          |
    | ?=              |          |
    | ?               |          |

Scenario Outline: Try to remove a non-existing element
    Given the query string '?param1=value1&param2=value2&param3=value3'
    When I remove '<remove>' from the query string
    Then the modified query string should be '?param1=value1&param2=value2&param3=value3'

    Examples:
    | remove   |
    | toremove |
    |          |

Scenario Outline: Try to remove an element when the query string does not start with a question mark and is not empty
    Given the query string '<querystring>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystring>'

    Examples:
    | querystring                                          |
    | obviouslynotaquerystring                             |
    | almost=a&query=string                                |
    | toremove=willremainbecausethisisnotavalidquerystring |

Scenario Outline: Remove multiple elements
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                                                          | querystringout                          |
    | ?toremove=this&toremove=this                                           |                                         |
    | ?toremove=this&toremove=this&param1=value1                             | ?param1=value1                          |
    | ?toremove=this&toremove=this&param1=value1&param2=value2               | ?param1=value1&param2=value2            |
    | ?toremove=this&param1=value1&toremove=this                             | ?param1=value1                          |
    | ?toremove=this&param1=value1&toremove=this&param2=value2               | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove=this&toremove=this                             | ?param1=value1                          |
    | ?param1=value1&toremove=this&toremove=this&param2=value2               | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove=this&param2=value2&toremove=this               | ?param1=value1&param2=value2            |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove=this | ?param1=value1&param2=value2            |
    | ?toremove=this&toremove=this&nameonly                                  | ?nameonly                               |
    | ?toremove=this&toremove=this&nameonly=                                 | ?nameonly=                              |
    | ?toremove=this&toremove=this&=valueonly                                | ?=valueonly                             |
    | ?toremove=this&toremove=this&nameonly&param1=value1                    | ?nameonly&param1=value1                 |
    | ?toremove=this&toremove=this&nameonly=&param1=value1                   | ?nameonly=&param1=value1                |
    | ?toremove=this&toremove=this&=valueonly&param1=value1                  | ?=valueonly&param1=value1               |
    | ?nameonly&param1=value1&toremove=this&toremove=this                    | ?nameonly&param1=value1                 |
    | ?nameonly=&param1=value1&toremove=this&toremove=this&param2=value2     | ?nameonly=&param1=value1&param2=value2  |
    | ?=valueonly&param1=value1&toremove=this&param2=value2&toremove=this    | ?=valueonly&param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove=this | ?param1=value1&param2=value2            |
    | ?toremove&toremove=this                                                |                                         |
    | ?toremove=this&toremove                                                |                                         |
    | ?toremove&toremove                                                     |                                         |
    | ?toremove&toremove=this&param1=value1                                  | ?param1=value1                          |
    | ?toremove=this&toremove&param1=value1                                  | ?param1=value1                          |
    | ?toremove=this&toremove=this&param1=value1&param2=value2               | ?param1=value1&param2=value2            |
    | ?toremove&toremove=this&param1=value1&param2=value2                    | ?param1=value1&param2=value2            |
    | ?toremove=this&toremove&param1=value1&param2=value2                    | ?param1=value1&param2=value2            |
    | ?toremove&param1=value1&toremove=this                                  | ?param1=value1                          |
    | ?toremove=this&param1=value1&toremove                                  | ?param1=value1                          |
    | ?toremove&param1=value1&toremove=this&param2=value2                    | ?param1=value1&param2=value2            |
    | ?toremove=this&param1=value1&toremove&param2=value2                    | ?param1=value1&param2=value2            |
    | ?toremove&param1=value1&toremove&param2=value2                         | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove&toremove=this                                  | ?param1=value1                          |
    | ?param1=value1&toremove=this&toremove                                  | ?param1=value1                          |
    | ?param1=value1&toremove&toremove                                       | ?param1=value1                          |
    | ?param1=value1&toremove&toremove=this&param2=value2                    | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove=this&toremove&param2=value2                    | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove&toremove&param2=value2                         | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove&param2=value2&toremove=this                    | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove=this&param2=value2&toremove                    | ?param1=value1&param2=value2            |
    | ?param1=value1&toremove&param2=value2&toremove                         | ?param1=value1&param2=value2            |
    | ?toremove&param1=value1&toremove=this&param2=value2&toremove=this      | ?param1=value1&param2=value2            |
    | ?toremove=this&param1=value1&toremove&param2=value2&toremove=this      | ?param1=value1&param2=value2            |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove      | ?param1=value1&param2=value2            |
    | ?toremove&param1=value1&toremove&param2=value2&toremove                | ?param1=value1&param2=value2            |
