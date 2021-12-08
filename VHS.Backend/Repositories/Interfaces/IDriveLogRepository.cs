using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Entities;

namespace VHS.Backend.Repositories.Interfaces
{
    public interface IDriveLogRepository
    {
        Task <IList<VehicleLogEntity>> GetLogs(DateTime filterStart, DateTime filterEnd, string vin);
        Task PostLog(VehicleLogEntity logEntry);
    }
}
