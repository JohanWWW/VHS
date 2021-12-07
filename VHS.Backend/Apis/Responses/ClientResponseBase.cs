using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace VHS.Backend.Apis.Responses
{
    public abstract class ClientResponseBase
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public bool IsStatusSuccess => StatusCode is HttpStatusCode.OK;
    }
}
