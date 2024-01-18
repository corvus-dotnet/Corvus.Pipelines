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

# Do we want both:
# obviouslynotaquerystring
# almost=a&query=string
Scenario: Try to remove an element when the query string does not start with a question mark and is not empty
    Given the query string 'obviouslynotaquerystring'
    When I remove 'toremove' from the query string
    Then the modified query string should be 'obviouslynotaquerystring'

# ?param1=value1&toremove=this&notvalid
# ?param1=value1&toremove=this&notvalid=
# ?param1=value1&toremove=this&=notvalid
# ?param1=value1&toremove=this&
Scenario Outline: Remove an element that is followed by garbage
    Given the query string '<querystringin>'
    When I remove 'toremove' from the query string
    Then the modified query string should be '<querystringout>'

    Examples:
    | querystringin                          | querystringout           |
    | ?param1=value1&toremove=this&notvalid  | ?param1=value1&notvalid  |
    | ?param1=value1&toremove=this&notvalid= | ?param1=value1&notvalid= |
    | ?param1=value1&toremove=this&=notvalid | ?param1=value1&=notvalid |
    | ?param1=value1&toremove=this&          | ?param1=value1&          |


# NEXT TIME:
# Remove multiple elements
# Consecutive, non-consecutive