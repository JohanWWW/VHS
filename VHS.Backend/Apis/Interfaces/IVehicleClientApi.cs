using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Apis.Responses;
using VHS.Utility.Types;

namespace VHS.Backend.Apis.Interfaces
{
    public interface IVehicleClientApi
    {
        Task<VehicleStatusResponse> GetStatus(string vin);
        Task<GeoCoordinate?> GetCurrentPosition(string vin);
        Task<bool> Blink(string vin);
        Task<bool> Beep(string vin);
        Task<bool> AddVehicle(string vin);
        Task<bool> Exists(string vin);
        IEnumerable<string> GetVins();
    }
}
