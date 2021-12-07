using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VHS.Backend.Apis;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Entities;

namespace VHS.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationClientApi _authorizationClientApi;

        public AuthorizationController()
        {
            _authorizationClientApi = new AuthorizationApi();
        }

        [HttpGet]
        public async Task<ActionResult<AuthorizationEntity>> Authorize([FromQuery] string username, [FromQuery] string password)
        {
            var response = await _authorizationClientApi.Authorize(username, password);

            if (!response.IsStatusSuccess)
                return new StatusCodeResult((int)response.StatusCode);

            return Ok(new AuthorizationEntity
            {
                AccessToken = response.AccessToken,
                CustomerId = response.CustomerId,
                DisplayName = response.DisplayName,
                Id = response.Id
            });
        }
    }
}
