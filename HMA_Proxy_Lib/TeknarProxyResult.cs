using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Teknar_Proxy_Lib
{
    public class TeknarProxyResult<T>
    {
        public Exception Error { get; set; }
        public TeknarProxy Proxy { get; set; }
        public string Url { get; set; }
        public string stringResult { get; set; }
        public T Result { get; set; }

        public bool isNotFoundError()
        {
            if (hasWebException())
            {
                var response = (HttpWebResponse)((WebException)Error).Response;
                return response != null && response.StatusCode == HttpStatusCode.NotFound;
            }
            return false;
        }

        public bool hasException()
        {
            return Error != null;
        }

        public bool hasWebException()
        {
            return hasException() && Error is WebException;
        }
    }

    public class TeknarMultipleConnectionError : Exception
    {
        public TeknarProxyResult<string>[] Results { get; set; }
        public TeknarMultipleConnectionError(string message, TeknarProxyResult<string>[] results)
            : base(message)
        {
            Results = results;
        }
    }
}