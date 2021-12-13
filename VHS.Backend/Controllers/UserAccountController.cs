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
        private readonly IVehicleClientApi _vehicleClientApi;

        public UserAccountController(IUserAccountClientApi userAccountClientApi, IVehicleClientApi vehicleClientApi)
        {
            _userAccountClientApi = userAccountClientApi;
            _vehicleClientApi = vehicleClientApi;
        }

        [HttpPost("register/{vin}/{customerId}/{accessToken}")]
        public async Task<ActionResult<VehicleEntity>> Register([FromRoute] string vin, [FromRoute] Guid customerId, [FromRoute] string accessToken)
        {
            VehicleClientResponse response;
            if (await _vehicleClientApi.AddVehicle(vin))
            {
                response = await _userAccountClientApi.Register(customerId, vin, accessToken);

                if (!response.IsStatusSuccess)
                    return new StatusCodeResult((int)response.StatusCode);

                return Ok(AutoMapper.Map<VehicleClientResponse, VehicleEntity>(response));
            }

            return BadRequest();
        }
    }
}
