using System.Net;

namespace VHS.Backend.Apis.Responses
{
    public abstract class ClientResponseBase
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public bool IsStatusSuccess => StatusCode is HttpStatusCode.OK;
    }
}
