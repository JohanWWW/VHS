using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Entities;
using VHS.Utility.Mapping;

namespace VHS.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationClientApi _authorizationClientApi;

        public AuthorizationController(IAuthorizationClientApi authorizationClientApi)
        {
            _authorizationClientApi = authorizationClientApi;
        }

        [HttpGet]
        public ActionResult<UserEntity> Authorize([FromQuery] string username, [FromQuery] string password)
        {
            var response = _authorizationClientApi.Authorize(username, password);

            if (!response.IsStatusSuccess)
            {
                return new StatusCodeResult((int)response.StatusCode);
            }

            ServiceProvider.Current.InMemoryStorage.AddToken(response.AccessToken, response.Id);

            return Ok(AutoMapper.Map<UserClientResponse, UserEntity>(response));
        }
    }
}
