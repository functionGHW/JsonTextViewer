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
using Microsoft.Win32;

namespace JsonTextViewer
{
    public class PageViewModel : ViewModelBase
    {
        private readonly IWebRequester requester;
        private string url;
        private string method;
        private string requestBody;
        private string responseText;
        private bool enableCookies;

        public PageViewModel() : this(new WebRequester())
        {
        }

        public PageViewModel(IWebRequester requester)
        {
            if (requester == null)
                throw new ArgumentNullException(nameof(requester));
            this.requester = requester;
            SendRequestCommand = new SimpleCommand(SendRequestCommandExecute);
            ViewInWebCommand = new SimpleCommand(ViewInWebCommandExecute);
            FormatJsonCommand = new SimpleCommand(FormatJsonCommandExecute);

            EnableCookies = true;
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

        public bool EnableCookies
        {
            get { return enableCookies; }
            set
            {
                if (value != enableCookies)
                {
                    enableCookies = value;
                    OnPropertyChanged();
                    requester.SetCookies(enableCookies);
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SendRequestCommand { get; set; }

        public ICommand ViewInWebCommand { get; set; }

        public ICommand FormatJsonCommand { get; set; }

        #endregion

        private void ViewInWebCommandExecute(object arg)
        {
            string tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".html");
            File.WriteAllText(tmpFile, this.ResponseText);

            System.Diagnostics.Process.Start(tmpFile);
        }

        private void FormatJsonCommandExecute(object arg)
        {
            string content = ResponseText;
            try
            {
                content = JToken.Parse(content).ToString();
                ResponseText = content;
            }
            catch (JsonReaderException ex)
            {
                ResponseText = $"Format ERROR! Content is not correct JSON string.\n\n{ex.Message}\n \n=======================\n{content}";
            }

        }


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
                bool saveAsFile = false;
                try
                {
                    var param = ParseBody(RequestBody, out headers, out saveAsFile);
                    if (saveAsFile)
                    {
                        var result = requester.SendDownloadRequest(Url, Method, param, headers);
                        SaveFile(result);
                    }
                    else
                    {
                        ResponseText = requester.SendRequest(Url, Method, param, headers);
                    }
                    UrlHistoriesManager.Instance.RefreshUrl(Url);
                }
                catch (JsonException ex)
                {
                    ResponseText = $"Request body format ERROR!\n\n{ex.Message}";
                }
                catch (WebRequesterException ex)
                {
                    ResponseText = $"Request ERROR!\n\n{ex.Message}";
                }
                catch (Exception ex)
                {
                    ResponseText = $"Something is wrong:\n\n{ex.Message}";
                }
            });
        }

        private void SaveFile(FileResult result)
        {
            if (result == null)
            {
                ResponseText = "null";
                return;
            }

            var saveFileDialog = new SaveFileDialog()
            {
                FileName = result.FileName
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string readableTotalSize = ConvertToReadableSize(result.FileLength);
                using (var fs = File.OpenWrite(filePath))
                {
                    using (var ns = result.FileStream)
                    {
                        fs.SetLength(0);

                        var buffer = new byte[81920];
                        long wroteSize = 0;
                        int count = 0;
                        while ((count = ns.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            fs.Write(buffer, 0, count);
                            wroteSize += count;
                            ResponseText = $"downlaoding...{ConvertToReadableSize(wroteSize)}/{readableTotalSize}";
                        }
                        ns.CopyTo(fs);
                    }
                }
                ResponseText = $"FileName={result.FileName}\nFileSize={result.FileLength}({readableTotalSize})\nLocal Path={filePath}";
            }
            else
            {
                result.FileStream?.Dispose();
                ResponseText = "Operation Canceled.";
            }
        }

        private string ConvertToReadableSize(long size)
        {
            const long KB = 1024;
            const long MB = 1024 * KB;
            const long GB = 1024 * MB;

            if (size > 4 * GB)
                return ((double)size / GB).ToString("0.##") + "GB";

            if (size > 10 * MB)
                return ((double)size / MB).ToString("0.##") + "MB";

            if (size > 10 * KB)
                return ((double)size / KB).ToString("0.##") + "KB";

            return size.ToString() + "Bytes";
        }

        private HttpContent ParseBody(string body, out Dictionary<string, string> headers, out bool saveAsFile)
        {
            headers = null;
            saveAsFile = false;
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
            saveAsFile = input.SaveAsFile;
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
            if (input.Body == null && input.Files == null)
                return null;

            // no file to upload, using FormUrlEncodedContent
            List<KeyValuePair<string, string>> parameters;
            if (input.Files.All(f => string.IsNullOrEmpty(f.Name)))
            {
                parameters = ParseFormDatas(input.Body);
                return new FormUrlEncodedContent(parameters);
            }

            // try to upload file using MultipartFormDataContent
            var content = new MultipartFormDataContent();
            parameters = ParseFormDatas(input.Body);
            foreach (var item in parameters)
            {
                content.Add(new StringContent(item.Value), item.Key);
            }
            foreach (var file in input.Files)
            {
                if (string.IsNullOrEmpty(file.Name))
                    throw new InvalidDataException("file.name must be given, and value can't be null or empty.");

                if (!File.Exists(file.Path))
                    throw new InvalidDataException("file.path is wrong, file not exists.");

                if (string.IsNullOrEmpty(file.FileName))
                    file.FileName = Path.GetFileName(file.Path);

                var fileContent = new StreamContent(File.OpenRead(file.Path));
                if (!string.IsNullOrEmpty(file.Type))
                {
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.Type);
                }
                content.Add(fileContent, file.Name, file.FileName);
            }
            return content;
        }

        private static List<KeyValuePair<string, string>> ParseFormDatas(JToken body)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            if (body != null)
            {
                foreach (JProperty item in body)
                {
                    string name = item.Name;
                    if (item.Value is JArray values)
                    {
                        foreach (var val in values)
                        {
                            parameters.Add(new KeyValuePair<string, string>(name, val.ToString()));
                        }
                    }
                    else
                    {
                        parameters.Add(new KeyValuePair<string, string>(name, item.Value.ToString()));
                    }
                }
            }

            return parameters;
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

        public bool SaveAsFile { get; set; }

        public List<FileData> Files { get; set; }
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
