using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;
using VHS.Utility.Types;

namespace VHS.Backend.Controllers
{
    [Route("api/vehicle")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IDriveLogRepository _driveLogRepository;
        private readonly IVehicleClientApi _vehicleClientApi;

        public VehicleController(IDriveLogRepository driveLogRepository, IVehicleClientApi vehicleClientApi)
        {
            _driveLogRepository = driveLogRepository;
            _vehicleClientApi = vehicleClientApi;
        }

        [Obsolete("Use " + nameof(Ping) + " instead")]
        [HttpGet("blinkAndBeep/{vin}")]
        public async Task<ActionResult<bool>> Beep([FromRoute] string vin)
        {
            return Ok(await _vehicleClientApi.Beep(vin) && await _vehicleClientApi.Blink(vin));
        }

        /// <summary>
        /// This endpoint is called whenever the client wants to ping (activate beeper and hazards)
        /// the vehicle
        /// </summary>
        /// <param name="clientPosition">The clients current position</param>
        [HttpPost("ping/{vin}")]
        public async Task<ActionResult<bool>> Ping([FromRoute] string vin, [FromBody] GeoCoordinate clientPosition)
        {
            GeoCoordinate? vehiclePosition = await _vehicleClientApi.GetCurrentPosition(vin);
            if (vehiclePosition is null)
                return NotFound($"Failed to establish a connection with car");

            double distance = GeoCoordinate.GetMetricDistance(clientPosition, (GeoCoordinate)vehiclePosition);
            if (distance > 0.200d)
                return NotFound($"Too far from car");

            return Ok(await _vehicleClientApi.Beep(vin) && await _vehicleClientApi.Blink(vin));
        }

        [HttpGet("status/{vin}")]
        public async Task<ActionResult<VehicleStatusResponse>> Status([FromRoute] string vin)
        {
            VehicleStatusResponse response = await _vehicleClientApi.GetStatus(vin);
            
            if (response is null)
                return NotFound($"Failed to establish a connection with car");

            return Ok(response);
        }
        
        [HttpGet("logs/{vin}")]
        public async Task<ActionResult<IList<VehicleLogEntity>>> GetLogs([FromRoute] string vin, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            return Ok(await _driveLogRepository.GetLogs(vin, start, end));
        }

        [Obsolete("Logs are posted by the car not the client")]
        [HttpPost("logs/{vin}")]
        public async Task<ActionResult<Guid>> PostLog([FromRoute] string vin, [FromBody] VehicleLogEntity logEntry)
        {
            return Ok(await _driveLogRepository.PostLog(vin, logEntry));
        }
    }
}
