using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VHS.Backend.Entities;
using VHS.Backend.Exceptions.Http;
using VHS.Backend.Repositories;
using VHS.Backend.Repositories.Interfaces;

namespace VHS.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationRepository _authorizationApi;

        public AuthorizationController()
        {
            _authorizationApi = new AuthorizationApi();
        }

        [HttpGet]
        public async Task<ActionResult<AuthorizationResponseEntity>> Authorize(string username, string password)
        {
            try
            {
                return await _authorizationApi.Authorize(username, password);
            }
            catch (HttpResponseException e)
            {
                return new StatusCodeResult((int)e.StatusCode);
            }
        }
    }
}
