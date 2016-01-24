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

namespace JsonTextViewer
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly IWebRequester requester;
        private string url;
        private string method;
        private string requestBody;
        private string responseText;

        public ViewModel() : this(new WebRequester())
        {
        }

        public ViewModel(IWebRequester requester)
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
                var param = ParseBody(RequestBody);
                ResponseText = requester.SendRequest(Url, Method, param);
            });
        }


        private HttpContent ParseBody(string body)
        {
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

            string firstLine = validLines.FirstOrDefault(line => line.Trim().Length > 0);

            string contentType = "text";
            if (firstLine != null && firstLine.StartsWith("::"))
            {
                contentType = firstLine.Substring(2).Trim().ToLowerInvariant();
                validLines.Remove(firstLine);
            }

            switch (contentType)
            {
                case "form":
                    return AsFormContent(validLines.Select(line => line.Trim())
                                                    .Where(line => line.Length > 0));
                case "json":
                    return AsJsonContent(validLines.Select(line => line.Trim())
                                                    .Where(line => line.Length > 0));
                case "text":
                    return AsTextContent(validLines);
            }

            return null;
        }

        private FormUrlEncodedContent AsFormContent(IEnumerable<string> text)
        {
            var dict = new Dictionary<string, string>();
            foreach (string line in text)
            {
                string[] kv = line.Split(new[] { '=' }, 2);
                if (kv.Length < 2)
                    continue;

                string name = kv[0].Trim();
                string value = kv[1].Trim();

                dict.Add(name, value);
            }
            return new FormUrlEncodedContent(dict);
        }

        private JsonContent AsJsonContent(IEnumerable<string> text)
        {
            string json = string.Concat(text);
            var obj =  JsonConvert.DeserializeObject<JObject>(json);
            return new JsonContent(obj);
        }

        private StringContent AsTextContent(IEnumerable<string> text)
        {
            return new StringContent(string.Join("\n", text));
        }

        #region INotifyPropertyChanged Implemenetation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var events = PropertyChanged;
            events?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
