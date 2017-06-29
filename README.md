# JsonTextViewer #
a simply WPF app to format json text that return from a web request.

Now support for file uploading and downloading.

Dependencies:

+ .Net Framework v4.5
+ Newtonsoft.Json v8.0.2

## About the message body ##
You can add headers for your requests by adding them to "headers";

For requests using POST or PUT, you can also send data by write it as "body";

1. Lines start with '#' as the first non-white space character are comments, these lines will be ignored.
2. If you don't nedd headers, keep it empty, or just remove it from json.
3. The type can be one of { text, form, json(application/json) }.
4. The 'form' support file uploading, by enable the 'file' block.
5. For file downloading request(or just want to save the result to a file), set 'SaveAsFile' to true.

Example for text
<pre>
    {
        headers: {
            # additional headers
            "uid": 1
        },
        # type can be one of { text, form, json }
        type: "text",
        
        # for text content, using string replace object 
        body: " this is a text content",
        SaveAsFile: false
    }
</pre>

Example for form:
<pre>
    {
        headers: null
        # type can be one of { text, form, json }
        type: "form",
        body: {
            name: "John",
            age: 23
        },
        SaveAsFile: false
    }
</pre>


Example for form(file uploading):
<pre>
    {
        headers: null
        # type can be one of { text, form, json }
        type: "form",
        body: {
            name: "John",
            age: 23
        },
        SaveAsFile: false,
        file: {
            name: "picture",
            path: "c:\\dir_to_file\\file_name.jpg",
            filename: "example.jpg",
            type: "image/jpg"
        }
    }
</pre>

Example for json:
<pre>
    {
        # without headers property
        # type can be one of { text, form, json }
        type: "json",
        body: {
            name: "John",
            age: 23
        }
        SaveAsFile: false
    }
</pre>
