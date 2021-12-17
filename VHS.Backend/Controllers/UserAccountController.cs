using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Attributes;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;
using VHS.Utility.Mapping;

namespace VHS.Backend.Controllers
{
    [VHSAuthorization]
    [Route("api/useraccount")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly IUserAccountClientApi _userAccountClientApi;
        private readonly IVehicleClientApi _vehicleClientApi;
        private readonly IDriveLogRepository _driveLogRepoitory;


        public UserAccountController(IUserAccountClientApi userAccountClientApi, IVehicleClientApi vehicleClientApi, IDriveLogRepository driveLogRepository)
        {
            _userAccountClientApi = userAccountClientApi;
            _vehicleClientApi = vehicleClientApi;
            _driveLogRepoitory = driveLogRepository;
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

        [HttpGet("Journal/{vin}")]
        public async Task<ActionResult<IList<ResultdriveJournalEntity>>> GetDriveJournal([FromRoute] string vin, [FromQuery] DateTime? start, [FromQuery] DateTime? end) { 
            var result = _driveLogRepoitory.GetDriveJournal(vin, start, end).ToList();
            return Ok(result);
        }
    }
}
