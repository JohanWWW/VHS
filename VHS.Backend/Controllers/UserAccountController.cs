using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Entities;

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

            return Ok(new VehicleEntity
            {
                Vin = response.Vin,
                RegNo = response.RegNo,
                Manufacturer = response.Manufacturer,
                Model = response.Model,
                Color = response.Color,
                Owner = (response.Owner is not null ? new VehicleEntity.VehicleOwner
                {
                    Id = response.Owner.Id,
                    FirstName = response.Owner.FirstName,
                    LastName = response.Owner.LastName,
                    City = response.Owner.City,
                    PhoneNumber = response.Owner.PhoneNumber,
                    User = (response.Owner.User is not null ? new UserEntity
                    {
                        Id = response.Owner.User.Id,
                        DisplayName = response.Owner.User.DisplayName,
                        AccessToken = response.Owner.User.AccessToken,
                        CustomerId = response.Owner.User.CustomerId
                    }
                    : null),
                    OwnerStatus = response.Owner.OwnerStatus
                }
                : null)
            });
        }
    }
}
