Feature: QueryStringManipulation

As a pipeline developer
I want to be able to modify the query string of a URL

Scenario: Remove a single element at the start
    Given the query string '?toremove=value&param2=value2&param3=value3'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

Scenario Outline: Remove a single malformed element at the start
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param2=value2&param3=value3'

    Examples:
    | querystringin                          |
    | ?toremove=&param2=value2&param3=value3 |
    | ?toremove&param2=value2&param3=value3  |

Scenario: Remove a single element in the middle
    Given the query string '?param1=value1&toremove=value&param3=value3'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

Scenario Outline: Remove a single malformed element in the middle
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param3=value3'

    Examples:
    | querystringin                          |
    | ?param1=value1&toremove=&param3=value3 |
    | ?param1=value1&toremove&param3=value3 |

Scenario: Remove a single element at the end
    Given the query string '?param1=value1&param2=value2&toremove=value'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param2=value2'

Scenario Outline: Remove a single malformed element at the end
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param2=value2'

    Examples:
    | querystringin                          |
    | ?param1=value1&param2=value2&toremove= |
    | ?param1=value1&param2=value2&toremove  |

Scenario: Remove the only element
    Given the query string '?toremove=value'
    When I remove 'toremove' from the query string
    Then the modified query string should be ''

Scenario: Try to remove a non-existing element
    Given the query string '?param1=value1&param2=value2&param3=value3'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1=value1&param2=value2&param3=value3'

Scenario: Try to remove an element appearing after an element with no equals symbol
    Given the query string '?param1&toremove=val&param3=value3'
    When I remove 'toremove' from the query string
    Then the modified query string should be '?param1&param3=value3'

Scenario Outline: Try to remove an element when the query string does not start with a question mark and is not empty
    Given the query string '<querystring>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystring>'

    Examples:
    | querystring                                          |
    | obviouslynotaquerystring                             |
    | almost=a&query=string                                |
    | toremove=willremainbecausethisisnotavalidquerystring |

Scenario Outline: Remove an element that is followed by garbage
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                           | querystringout            |
    | ?param1=value1&toremove=this&notvalid   | ?param1=value1&notvalid   |
    | ?param1=value1&toremove=this&notvalid=  | ?param1=value1&notvalid=  |
    | ?param1=value1&toremove=this&notvalid== | ?param1=value1&notvalid== |
    | ?param1=value1&toremove=this&notva=lid= | ?param1=value1&notva=lid= |
    | ?param1=value1&toremove=this&notva=li=d | ?param1=value1&notva=li=d |
    | ?param1=value1&toremove=this&=notvalid  | ?param1=value1&=notvalid  |
    | ?param1=value1&toremove=this&           | ?param1=value1&           |


Scenario Outline: Remove an element that follows garbage
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                           | querystringout            |
    | ?param1=value1&notvalid&toremove=this   | ?param1=value1&notvalid   |
    | ?param1=value1&notvalid=&toremove=this  | ?param1=value1&notvalid=  |
    | ?param1=value1&notvalid==&toremove=this | ?param1=value1&notvalid== |
    | ?param1=value1&notva=lid=&toremove=this | ?param1=value1&notva=lid= |
    | ?param1=value1&notva=li=d&toremove=this | ?param1=value1&notva=li=d |
    | ?param1=value1&=notvalid&toremove=this  | ?param1=value1&=notvalid  |
    | ?param1=value1&&toremove=this           | ?param1=value1&           |

Scenario Outline: Remove an element after good then garbage
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                           | querystringout            |
    | ?notvalid&param1=value1&toremove=this   | ?notvalid&param1=value1   |
    | ?notvalid=&param1=value1&toremove=this  | ?notvalid=&param1=value1  |
    | ?notvalid==&param1=value1&toremove=this | ?notvalid==&param1=value1 |
    | ?notva=lid=&param1=value1&toremove=this | ?notva=lid=&param1=value1 |
    | ?notva=li=d&param1=value1&toremove=this | ?notva=li=d&param1=value1 |
    | ?=notvalid&param1=value1&toremove=this  | ?=notvalid&param1=value1  |
    | ?&param1=value1&toremove=this           | ?&param1=value1           |

Scenario Outline: Remove multiple elements
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                                                          | querystringout               |
    | ?toremove=this&toremove=this                                           |                              |
    | ?toremove=this&toremove=this&param1=value1                             | ?param1=value1               |
    | ?toremove=this&toremove=this&param1=value1&param2=value2               | ?param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove=this                             | ?param1=value1               |
    | ?toremove=this&param1=value1&toremove=this&param2=value2               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove=this&toremove=this                             | ?param1=value1               |
    | ?param1=value1&toremove=this&toremove=this&param2=value2               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove=this&param2=value2&toremove=this               | ?param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove=this | ?param1=value1&param2=value2 |


Scenario Outline: Remove multiple elements retaining some garbage
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                                                          | querystringout                      |
    | ?toremove=this&toremove=this&wrong                                     | ?wrong                               |
    | ?toremove=this&toremove=this&wrong=                                    | ?wrong=                              |
    | ?toremove=this&toremove=this&=wrong                                    | ?=wrong                              |
    | ?toremove=this&toremove=this&wrong&param1=value1                       | ?wrong&param1=value1                |
    | ?toremove=this&toremove=this&wrong=&param1=value1                      | ?wrong=&param1=value1               |
    | ?toremove=this&toremove=this&=wrong&param1=value1                      | ?=wrong&param1=value1               |
    | ?wrong&param1=value1&toremove=this&toremove=this                       | ?wrong&param1=value1                |
    | ?wrong=&param1=value1&toremove=this&toremove=this&param2=value2        | ?wrong=&param1=value1&param2=value2 |
    | ?=wrong&param1=value1&toremove=this&param2=value2&toremove=this        | ?=wrong&param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove=this | ?param1=value1&param2=value2        |


Scenario Outline: Remove multiple garbage elements
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                                                     | querystringout               |
    | ?toremove&toremove=this                                           |                              |
    | ?toremove=this&toremove                                           |                              |
    | ?toremove&toremove                                                |                              |
    | ?toremove&toremove=this&param1=value1                             | ?param1=value1               |
    | ?toremove=this&toremove&param1=value1                             | ?param1=value1               |
    | ?toremove=this&toremove=this&param1=value1&param2=value2          | ?param1=value1&param2=value2 |
    | ?toremove&toremove=this&param1=value1&param2=value2               | ?param1=value1&param2=value2 |
    | ?toremove=this&toremove&param1=value1&param2=value2               | ?param1=value1&param2=value2 |
    | ?toremove&param1=value1&toremove=this                             | ?param1=value1               |
    | ?toremove=this&param1=value1&toremove                             | ?param1=value1               |
    | ?toremove&param1=value1&toremove=this&param2=value2               | ?param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove&param2=value2               | ?param1=value1&param2=value2 |
    | ?toremove&param1=value1&toremove&param2=value2                    | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove&toremove=this                             | ?param1=value1               |
    | ?param1=value1&toremove=this&toremove                             | ?param1=value1               |
    | ?param1=value1&toremove&toremove                                  | ?param1=value1               |
    | ?param1=value1&toremove&toremove=this&param2=value2               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove=this&toremove&param2=value2               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove&toremove&param2=value2                    | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove&param2=value2&toremove=this               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove=this&param2=value2&toremove               | ?param1=value1&param2=value2 |
    | ?param1=value1&toremove&param2=value2&toremove                    | ?param1=value1&param2=value2 |
    | ?toremove&param1=value1&toremove=this&param2=value2&toremove=this | ?param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove&param2=value2&toremove=this | ?param1=value1&param2=value2 |
    | ?toremove=this&param1=value1&toremove=this&param2=value2&toremove | ?param1=value1&param2=value2 |
    | ?toremove&param1=value1&toremove&param2=value2&toremove           | ?param1=value1&param2=value2 |
