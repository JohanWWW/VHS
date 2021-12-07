using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.Backend.Apis.Responses
{
    public class AuthorizationClientResponse : ClientResponseBase
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string AccessToken { get; set; }
        public Guid CustomerId { get; set; }
    }
}
