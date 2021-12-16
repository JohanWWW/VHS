using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;
using VHS.Utility.Mapping;
using VHS.Utility.Types;

namespace VHS.Backend.Repositories
{
    public class FakeDriveLogDB : IDriveLogRepository
    {
        private const string DB_FILE_PATH                           = "fakedrivelog.db";
        private const string CSV_DELIMITER                          = ",";

        private static readonly IFormatProvider _culture            = System.Globalization.CultureInfo.InvariantCulture;

        private readonly IDictionary<string, IList<DBLogEntity>> _storage;

        public FakeDriveLogDB()
        {
            _storage = new Dictionary<string, IList<DBLogEntity>>();

            if (!File.Exists(DB_FILE_PATH))
                File.Create(DB_FILE_PATH).Close();

            // Populate runtime storage from file
            foreach (string row in File.ReadLines(DB_FILE_PATH))
            {
                string[] cols = row.Split(CSV_DELIMITER);

                if (!_storage.ContainsKey(cols[0]))
                    _storage.Add(cols[0], new List<DBLogEntity>());

                string vin = cols[0];

                _storage[vin].Add(new DBLogEntity
                {
                    Id          = Guid.Parse(cols[1]),
                    LogDate     = DateTimeOffset.Parse(cols[2]),
                    IsDriving   = int.Parse(cols[3]) is not 0,
                    Mileage     = double.Parse(cols[4], _culture),
                    Position    = new GeoCoordinate
                    {
                        Latitude    = double.Parse(cols[5], _culture),
                        Longitude   = double.Parse(cols[6], _culture)
                    },
                    Battery     = new Battery
                    {
                        Level       = double.Parse(cols[7], _culture)
                    }
                });
            }
        }

        public async Task<IList<VehicleLogEntity>> GetLogs(string vin, DateTime? filterStart, DateTime? filterEnd)
        {
            if (!_storage.ContainsKey(vin))
                return null;

            if (_storage.Count is 0)
                return new List<VehicleLogEntity>();

            return _storage[vin]
                .Where(entry => entry.LogDate >= (filterStart ?? DateTimeOffset.MinValue) &&
                                entry.LogDate < (filterEnd ?? DateTimeOffset.MaxValue))
                .Select(entry => (VehicleLogEntity)entry)
                .ToList();
        }

        public async Task<Guid?> PostLog(string vin, VehicleLogEntity logEntry)
        {
            if (!IsCached(vin)) // New VIN?
                _storage.Add(vin, new List<DBLogEntity>());

            Guid id = Guid.NewGuid();

            DBLogEntity log = AutoMapper.Map<VehicleLogEntity, DBLogEntity>(logEntry);
            log.Id = id;
            log.LogDate = DateTimeOffset.Now;

            WriteToCache(vin, log);
            WriteToFile(vin, log);

            return id;
        }

        /// <summary>
        /// Writes log entry to db file
        /// </summary>
        /// <param name="vin">Vehicle Identifier Number</param>
        /// <param name="logEntry">The vehicle log entry</param>
        private static void WriteToFile(string vin, DBLogEntity logEntry)
        {
            using StreamWriter fileOut = File.AppendText(DB_FILE_PATH);
            fileOut.WriteLine(string.Join(CSV_DELIMITER,
                vin,
                logEntry.Id.ToString(),
                logEntry.LogDate.ToString("O"),
                logEntry.IsDriving ? "1" : "0",
                logEntry.Mileage.ToString(_culture),
                logEntry.Position.Latitude.ToString(_culture),
                logEntry.Position.Longitude.ToString(_culture),
                logEntry.Battery.Level.ToString(_culture)
            ));
            fileOut.Close();
        }

        /// <summary>
        /// Writes log entry to runtime storage
        /// </summary>
        /// <param name="vin">Vehicle Identifier Number</param>
        /// <param name="logEntry">The vehicle log entry</param>
        private void WriteToCache(string vin, DBLogEntity logEntry)
        {
            IList<DBLogEntity> entries = _storage[vin];
            entries.Add(logEntry);
        }

        private bool IsCached(string vin) => _storage.ContainsKey(vin);

        public IEnumerable<ResultdriveJournalEntity> GetDriveJournal(string vin, DateTime? filterStart, DateTime? filterEnd)
        {
            VehicleLogEntity startEntity, endEntity;
            IList<VehicleLogEntity> logs = GetLogs(vin, filterStart, filterEnd).Result;

            var batches = GetLogEntryBatches(logs);
            foreach (var batch in batches)
            {
                if (batch.Count < 2)
                    continue;

                startEntity = batch.First();
                endEntity = batch.Last();

                double averageSpeed = 0d;
                for (int i = 1; i < batch.Count; i++)
                {
                    double distance = GeoCoordinate.GetMetricDistance(batch[i - 1].Position, batch[i].Position);
                    averageSpeed += (distance / (batch[i].LogDate - batch[i - 1].LogDate).TotalHours) / batch.Count;
                }

                yield return new ResultdriveJournalEntity
                {
                    StartTime = startEntity.LogDate,
                    EndTime = endEntity.LogDate,
                    AverageSpeed = averageSpeed,
                    TotalDistance = endEntity.Mileage - startEntity.Mileage,
                    LogCount = batch.Count,
                    EnergyConsumption = startEntity.Battery.Level - endEntity.Battery.Level
                };
            }
        }

        private static IEnumerable<IList<VehicleLogEntity>> GetLogEntryBatches(IList<VehicleLogEntity> logs)
        {
            int i = 0;
            while (i < logs.Count)
            {
                var list = new List<VehicleLogEntity> { logs[i] };

                int k = i + 1;
                while (k < logs.Count && logs[k].IsDriving)
                {
                    list.Add(logs[k]);
                    k++;
                }

                i = k;

                yield return list;
            }
        }

        #region Scoped Types
        private class DBLogEntity : VehicleLogEntity
        {
            public Guid Id { get; set; }
        }
        #endregion
    }
}
