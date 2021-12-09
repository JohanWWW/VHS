using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VHS.Backend.Apis;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Entities;
using VHS.Utility.Mapping;

namespace VHS.Backend.Controllers
{
    [Route("api/useraccount")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly IUserAccountClientApi _userAccountClientApi;

        public UserAccountController()
        {
            _userAccountClientApi = new UserAccountApi();
        }

        [HttpPost("/{vin}/{customerId}/{accessToken}")]
        public async Task<ActionResult<VehicleEntity>> Register([FromRoute] string vin, [FromRoute] Guid customerId, [FromRoute] string accessToken)
        {
            var response = await _userAccountClientApi.Register(customerId, vin, accessToken);

            if (!response.IsStatusSuccess)
                return new StatusCodeResult((int)response.StatusCode);

            return Ok(AutoMapper.Map<VehicleClientResponse, VehicleEntity>(response));
        }
    }
}
