using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;

namespace VHS.Backend.Filters
{
    public class ClaimFilter : IAuthorizationFilter
    {
        private readonly IAuthorizationClientApi _authorizationClientApi;

        public ClaimFilter(IAuthorizationClientApi authorizationClientApi)
        {
            _authorizationClientApi = authorizationClientApi;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasClaim = false;
            var headerContent = context.HttpContext.Request.Headers["x-vhs-auth"];
            if (headerContent.Count == 1)
            {
                var token = headerContent[0];
                if (!string.IsNullOrEmpty(token) && ServiceProvider.Current.InMemoryStorage.TryGetUserId(token, out var userId))
                {
                    hasClaim = _authorizationClientApi.ValidateToken(userId, token);

                }
            }

            if (!hasClaim)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
