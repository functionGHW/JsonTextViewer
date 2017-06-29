using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonTextViewer
{
    [Serializable]
    class WebRequesterException : Exception
    {
        public WebRequesterException(string message) : base(message)
        {

        }
    }
}
