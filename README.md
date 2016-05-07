# JsonTextViewer #
a simply WPF app to format json text that return from a web request.

## About the message body ##
You can add a body for Post and Put requests;

1. Lines start with '#' as the first non-white space character are comments, these lines will be ignored.
2. Default type of the body is text which means text/plain, you can change it by add a line ":: {type}" as the first valid line of the content.
3. The type can be one of { text, form(application/x-www-form-urlencoded), json(application/json) }.

Example for text
<pre>
    :: text
    # this line is a comment.
    This is the content of the body, as a string.
</pre>

Example for form:
<pre>
    :: form
    # split by '='
    name=John
    age=23
</pre>
Example for json:
<pre>
    :: json
    # a json object
    {
        # a property named name
        "name": "John",
        # a property named age
        "age": 23
    }
</pre>
