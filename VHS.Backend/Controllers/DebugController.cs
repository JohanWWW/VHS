using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Attributes;

namespace VHS.Backend.Controllers
{
#if DEBUG
    [VHSAuthorization]
    [Route("api/DEBUG")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly IVehicleClientApi _vehicleClientApi;

        public DebugController(IVehicleClientApi vehicleClientApi)
        {
            _vehicleClientApi = vehicleClientApi;
        }

        [HttpPatch("vehicle/{vin}/isdriving/{bit}")]
        public async Task<ActionResult> SetIsVehicleDriving([FromRoute] string vin, [FromRoute] bool bit)
        {
            return Ok(await _vehicleClientApi.SetIsDriving(vin, bit));
        }

        [HttpPatch("vehicle/{vin}/resetbattery")]
        public async Task<ActionResult> ResetBattery([FromRoute] string vin)
        {
            return Ok(await _vehicleClientApi.ResetBattery(vin));
        }
    }
#endif
}
