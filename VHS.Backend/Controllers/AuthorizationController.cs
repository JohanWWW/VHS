using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Entities;
using VHS.Backend.Storage.Interfaces;
using VHS.Utility.Mapping;

namespace VHS.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationClientApi _authorizationClientApi;
        private readonly IAuthorizationStorage _authorizationStorage;

        public AuthorizationController(IAuthorizationClientApi authorizationClientApi, IAuthorizationStorage authorizationStorage)
        {
            _authorizationClientApi = authorizationClientApi;
            _authorizationStorage = authorizationStorage;
        }

        [HttpGet]
        public ActionResult<UserEntity> Authorize([FromQuery] string username, [FromQuery] string password)
        {
            var response = _authorizationClientApi.Authorize(username, password);

            if (!response.IsStatusSuccess)
            {
                return new StatusCodeResult((int)response.StatusCode);
            }

            _authorizationStorage.AddToken(response.AccessToken, response.Id);

            return Ok(AutoMapper.Map<UserClientResponse, UserEntity>(response));
        }
    }
}
