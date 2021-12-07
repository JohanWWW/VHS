using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.VehicleTest;

namespace VHS.Backend.Controllers
{
    [Route("api/vehicle")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicle _vehicle;

        public VehicleController()
        {
            _vehicle = new CloudCar();
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
        
    }
}
