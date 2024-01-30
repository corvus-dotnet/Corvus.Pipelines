Feature: UriPathEncoding

As a pipeline developer
I want to be able to obtain the path and query for a RequestSignature a form suitable for use in HTTP requests and responses

# There are two use cases currently:
#   Location header for redirect URL (prefix is Empty for this one, because we use it in the OIDC callback, which is an intra-site redirect)
#   Back end URL (fully qualified URL)

Scenario Outline: Encodes path and query when necessary
    Given the prefix, path, and query string '<Prefix in>', '<Path in>', and '<Encoded query string in>'
    When I append the prefix, encoded path, and query
    Then the result URL should be '<Result>'

    Examples:
    | Prefix in              | Path in          | Encoded query string in | Result                                          |
    |                        | /                |                         | /                                               |
    | http://example.com:80  | /                |                         | http://example.com:80/                          |
    | http://example.com:80/ | /                |                         | http://example.com:80/                          |
    |                        | /                | ?foo=bar                | /?foo=bar                                       |
    | http://example.com:80  | /                | ?foo=bar                | http://example.com:80/?foo=bar                  |
    | http://example.com:80  | /                | ?fo%25o=bar             | http://example.com:80/?fo%25o=bar               |
    | http://example.com:80  | /                | ?fo%23o=bar             | http://example.com:80/?fo%23o=bar               |
    |                        | /one/two         |                         | /one/two                                        |
    |                        | /one/two/        |                         | /one/two/                                       |
    | http://example.com:80  | /one/two         |                         | http://example.com:80/one/two                   |
    | http://example.com:80  | /one/two         | ?foo=bar                | http://example.com:80/one/two?foo=bar           |
    | http://example.com:80  | /one/two         | ?fo%25o=bar             | http://example.com:80/one/two?fo%25o=bar        |
    |                        | /one two         |                         | /one%20two                                      |
    | http://example.com:80  | /one two         |                         | http://example.com:80/one%20two                 |
    | http://example.com:80  | /one two         | ?foo=bar                | http://example.com:80/one%20two?foo=bar         |
    | http://example.com:80  | /one two         | ?fo%25o=bar             | http://example.com:80/one%20two?fo%25o=bar      |
    |                        | / one            |                         | /%20one                                         |
    |                        | /one two`        |                         | /one%20two%60                                   |
    |                        | /one two`three   |                         | /one%20two%60three                              |
    |                        | /one two%20three |                         | /one%20two%2520three                            |
    |                        | /one£two         |                         | /one%C2%A3two                                   |
    |                        | /one💩two       |                         | /one%F0%9F%92%A9two                              |
    | http://example.com:80  | /one💩two       |                         | http://example.com:80/one%F0%9F%92%A9two         |
    | http://example.com:80  | /one💩two       | ?foo=bar                | http://example.com:80/one%F0%9F%92%A9two?foo=bar |
