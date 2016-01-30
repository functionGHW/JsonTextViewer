# JsonTextViewer #
a simply WPF app to format json text that return from a web request.

## About the message body ##
You can add a body for Post and Put requests;<br />
1.Lines start with '#' as the first non-white space character are comments, these lines will be ignored.<br />
2.Default type of the body is text which means text/plain, you can change it by add a line ":: {type}" as the first valid line of the content.<br />
3.The type can be one of { text, form(application/x-www-form-urlencoded), json(application/json) }.<br />
Example for form:<br />
    :: form<br />
	# split by '='<br />
    name=John<br />
    Age=23<br />
