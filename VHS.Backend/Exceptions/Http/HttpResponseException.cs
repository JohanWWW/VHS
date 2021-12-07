using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace VHS.Backend.Exceptions.Http
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpResponseException() : base()
        {
        }

        public HttpResponseException(HttpStatusCode statusCode, Exception innerException = null) : base($"Status code {statusCode} was returned from the request", innerException)
        {
            StatusCode = statusCode;
        }
    }
}
