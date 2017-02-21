/* 
 * FileName:    IWebRequester.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  1/24/2016 2:01:17 PM
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JsonTextViewer
{
    public interface IWebRequester
    {
        string SendRequest(string url, string method, HttpContent content, Dictionary<string, string> headers);
    }
}
