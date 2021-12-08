using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;

namespace VHS.Backend.Repositories
{
    public class FakeDriveLogDB : IDriveLogRepository
    {
        private Dictionary<string, List>
        public Task<IList<VehicleLogEntity>> GetLogs(DateTime filterStart, DateTime filterEnd, string vin)
        {
            throw new NotImplementedException();
        }

        public Task PostLog(VehicleLogEntity logEntry)
        {
            throw new NotImplementedException();
        }
    }
}
