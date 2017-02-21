﻿/* 
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
        }

        private static readonly HttpContent EmptyContent = new ByteArrayContent(new byte[0]);

        public string SendRequest(string url, string method, HttpContent content = null, Dictionary<string,string> headers = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            try
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

                using (var client = new HttpClient())
                {
                    var response = client.SendAsync(request).Result;
                    if (!response.IsSuccessStatusCode)
                        return
                            $"Http {(int)response.StatusCode} {response.ReasonPhrase}\n{response.Content.ReadAsStringAsync().Result}";

                    var rContent = response.Content;

                    if (rContent == null)
                    {
                        return "null";
                    }

                    string type = rContent.Headers?.ContentType?.MediaType?.ToLowerInvariant();
                    if (type != null && type.Contains("json"))
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

        private string ReadAsJson(HttpContent content)
        {
            string json = content.ReadAsStringAsync().Result;
            var obj = JsonConvert.DeserializeObject(json);
            return obj?.ToString() ?? "null";
        }
    }
}
