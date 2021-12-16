using System;
using System.Threading.Tasks;
using VHS.Backend.Apis.Responses;

namespace VHS.Backend.Apis.Interfaces
{
    public interface IUserAccountClientApi
    {
        Task<VehicleClientResponse> Register(Guid customerId, string vin, string accessToken);
    }
}
