/* 
 * FileName:    ViewModel.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  1/24/2016 11:05:40 AM
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http.Headers;

namespace JsonTextViewer
{
    public class PageViewModel : ViewModelBase
    {
        private readonly IWebRequester requester;
        private string url;
        private string method;
        private string requestBody;
        private string responseText;

        public PageViewModel() : this(new WebRequester())
        {
        }

        public PageViewModel(IWebRequester requester)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester));
            this.requester = requester;
            SendRequestCommand = new SimpleCommand(SendRequestCommandExecute);
        }

        #region Properties

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }

        public string RequestBody
        {
            get { return requestBody; }
            set
            {
                requestBody = value;
                OnPropertyChanged();
            }
        }

        public string ResponseText
        {
            get { return responseText; }
            set
            {
                responseText = value;
                OnPropertyChanged();
            }
        }

        public string Method
        {
            get { return method; }
            set
            {
                method = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand SendRequestCommand { get; set; }

        #endregion

        private void SendRequestCommandExecute(object arg)
        {
            if (string.IsNullOrEmpty(url))
                ResponseText = "Input the url!";

            if (string.IsNullOrEmpty(method))
                ResponseText = "Select a method!";

            ResponseText = "Handling...";
            Task.Run(() =>
            {
                Dictionary<string, string> headers;
                try
                {
                    var param = ParseBody(RequestBody, out headers);
                    ResponseText = requester.SendRequest(Url, Method, param, headers);
                    UrlHistoriesManager.Instance.RefreshUrl(Url);
                }
                catch (JsonException ex)
                {
                    ResponseText = $"Request body format ERROR!\n\n{ex.Message}";
                }
                catch (Exception ex)
                {
                    ResponseText = $"Something is wrong:\n\n{ex.Message}";
                }
            });
        }


        private HttpContent ParseBody(string body, out Dictionary<string, string> headers)
        {
            headers = null;
            if (body == null)
                return null;

            string[] lines = body.Split('\n');

            var validLines = (from line in lines
                              let s = line.Trim()
                              where s.FirstOrDefault() != '#'
                              select line)
                              .ToList();
            if (!validLines.Any())
                return null;

            var input = JsonConvert.DeserializeObject<InputBody>(string.Concat(validLines));
            if (input == null)
                return null;

            headers = input.Headers;
            switch (input.Type)
            {
                case "form":
                    return AsFormContent(input);
                case "json":
                    return AsJsonContent(input);
                case "text":
                    return AsTextContent(input);
            }

            return null;
        }

        private HttpContent AsFormContent(InputBody input)
        {
            if (input.Body == null && input.File == null)
                return null;

            // no file to upload, using FormUrlEncodedContent
            if (input.File == null || string.IsNullOrEmpty(input.File.Name))
            {
                var dict = input.Body?.ToObject<Dictionary<string, string>>();
                return new FormUrlEncodedContent(dict);
            }
            else
            {
                // try to upload file using MultipartFormDataContent
                var file = input.File;
                if (string.IsNullOrEmpty(file.Name))
                    throw new InvalidDataException("file.name must be given, and value can't be null or empty.");

                if (!File.Exists(file.Path))
                    throw new InvalidDataException("file.path is wrong, file not exists.");

                if (string.IsNullOrEmpty(file.FileName))
                    file.FileName = Path.GetFileName(file.Path);

                var content = new MultipartFormDataContent();

                var dict = input.Body?.ToObject<Dictionary<string, string>>();
                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        content.Add(new StringContent(item.Value), item.Key);
                    }
                }
                var fileContent = new StreamContent(File.OpenRead(file.Path));
                if (!string.IsNullOrEmpty(file.Type))
                {
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.Type);
                }
                content.Add(fileContent, file.Name, file.FileName);

                return content;
            }
        }

        private HttpContent AsJsonContent(InputBody input)
        {
            if (input.Body == null)
                return null;

            return new JsonContent(input.Body);
        }

        private HttpContent AsTextContent(InputBody input)
        {
            if (input.Body == null)
                return null;

            return new StringContent(input.Body.ToString());
        }


    }

    public class InputBody
    {
        public string Type { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public JToken Body { get; set; }

        public FileData File { get; set; }
    }

    // for uploading file 
    public class FileData
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string FileName { get; set; }

        public string Type { get; set; }
    }
}
