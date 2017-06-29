/* 
 * FileName:    WebRequester.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  1/24/2016 3:28:35 PM
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace JsonTextViewer
{
    public class WebRequester : IWebRequester
    {
        static WebRequester()
        {
            // ignore SSL certificate, this may result to AuthenticationException exception.
            ServicePointManager
                .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            // add all protocols
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.Ssl3;
        }

        private static readonly HttpContent EmptyContent = new ByteArrayContent(new byte[0]);

        public FileResult SendDownloadRequest(string url, string method, HttpContent content = null, Dictionary<string, string> headers = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var request = BuildRequestMessage(url, method, content, headers);
            var client = new HttpClient();
            var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            var rContent = response.Content;
            if (!response.IsSuccessStatusCode)
            {
                string textContent = IsJsonContent(rContent) ? ReadAsJson(rContent) : rContent.ReadAsStringAsync().Result;
                throw new WebRequesterException($"Http {(int)response.StatusCode} {response.ReasonPhrase}\n{textContent}");
            }
            if (rContent == null)
            {
                return null;
            }

            string fileName = rContent.Headers?.ContentDisposition?.FileName;
            long fileLength = rContent.Headers.ContentLength ?? 0L;

            var result = new FileResult()
            {
                FileName = fileName,
                FileLength = fileLength,
                FileStream = rContent.ReadAsStreamAsync().Result
            };
            return result;
        }

        public string SendRequest(string url, string method, HttpContent content = null, Dictionary<string, string> headers = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            try
            {
                var request = BuildRequestMessage(url, method, content, headers);
                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                    var rContent = response.Content;
                    if (!response.IsSuccessStatusCode)
                    {
                        string textContent = IsJsonContent(rContent) ? ReadAsJson(rContent) : rContent.ReadAsStringAsync().Result;
                        return $"Http {(int)response.StatusCode} {response.ReasonPhrase}\n{textContent}";
                    }
                    if (rContent == null)
                    {
                        return "null";
                    }

                    if (IsJsonContent(rContent))
                    {
                        return ReadAsJson(rContent);
                    }
                    return rContent.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static HttpRequestMessage BuildRequestMessage(string url, string method, HttpContent content, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            switch (method.ToLowerInvariant())
            {
                // only post and put request can has a message body
                case "post":
                case "put":
                    request.Content = content ?? EmptyContent;
                    break;
            }

            return request;
        }

        private bool IsJsonContent(HttpContent rContent)
        {
            string type = rContent?.Headers?.ContentType?.MediaType?.ToLowerInvariant();
            return type != null && type.Contains("json");
        }

        private string ReadAsJson(HttpContent content)
        {
            string json = content.ReadAsStringAsync().Result;
            try
            {
                // formating json content
                var obj = JsonConvert.DeserializeObject(json);
                return obj?.ToString() ?? "null";
            }
            catch (JsonReaderException ex)
            {
                return $"Wrong JSON received, check format of the raw content!!!\nMessage:{ex.Message}\n\nRaw content is:\n{json}";
            }
        }
    }
}
