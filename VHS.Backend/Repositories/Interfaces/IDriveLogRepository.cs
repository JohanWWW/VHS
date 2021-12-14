using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VHS.Backend.Entities;

namespace VHS.Backend.Repositories.Interfaces
{
    public interface IDriveLogRepository
    {
        Task<IList<VehicleLogEntity>> GetLogs(string vin, DateTime? filterStart, DateTime? filterEnd);
        Task<Guid?> PostLog(string vin, VehicleLogEntity logEntry);

        Task<IList<ResultdriveJournalEntity>> GetDriveJournal(string vin, DateTime? filterStart, DateTime? filterEnd);
    }
}
