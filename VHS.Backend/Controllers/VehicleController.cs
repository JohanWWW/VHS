using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;
using VHS.VehicleTest;

namespace VHS.Backend.Controllers
{
    [Route("api/vehicle")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicle _vehicle;
        private readonly IDriveLogRepository _driveLogRepository;

        public VehicleController(IVehicle vehicle, IDriveLogRepository driveLogRepository)
        {
            _vehicle = vehicle;
            _driveLogRepository = driveLogRepository;
        }

        [HttpGet("blinkAndBeep")]
        public ActionResult<bool> Beep()
        {
            return Ok(_vehicle.Blink() && _vehicle.Beep());
        }

        [HttpGet("status")]
        public ActionResult<IVehicle> Status()
        {
            _vehicle.DriveSimulator();
            return Ok(_vehicle);
        }
        
        [HttpGet("logs/{vin}")]
        public async Task<ActionResult<IList<VehicleLogEntity>>> GetLogs([FromRoute] string vin, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            return Ok(await _driveLogRepository.GetLogs(vin, start, end));
        }

        [HttpPost("logs/{vin}")]
        public async Task<ActionResult<Guid>> PostLog([FromRoute] string vin, [FromBody] VehicleLogEntity logEntry)
        {
            return Ok(await _driveLogRepository.PostLog(vin, logEntry));
        }
    }
}
