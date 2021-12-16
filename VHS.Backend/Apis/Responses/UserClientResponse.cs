using System;

namespace VHS.Backend.Apis.Responses
{
    public class UserClientResponse : ClientResponseBase
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string AccessToken { get; set; }
        public Guid CustomerId { get; set; }
    }
}
