using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Entities;
using VHS.Backend.Repositories.Interfaces;
using VHS.VehicleTest;

namespace VHS.Backend.Repositories
{
    public class FakeDriveLogDB : IDriveLogRepository
    {
        private static readonly IFormatProvider INVARIANT_CULTURE   = System.Globalization.CultureInfo.InvariantCulture;
        private const string DB_FILE_PATH                           = "fakedrivelog.db";
        private const string SEPARATOR_TOKEN                        = ",";

        private readonly IDictionary<string, IList<DBLogEntity>> _storage;

        public FakeDriveLogDB()
        {
            _storage = new Dictionary<string, IList<DBLogEntity>>();

            if (!File.Exists(DB_FILE_PATH))
                File.Create(DB_FILE_PATH).Close();

            // Populate runtime storage from file
            foreach (string row in File.ReadLines(DB_FILE_PATH))
            {
                string[] cols = row.Split(SEPARATOR_TOKEN);

                if (!_storage.ContainsKey(cols[0]))
                    _storage.Add(cols[0], new List<DBLogEntity>());

                _storage[cols[0]].Add(new DBLogEntity
                {
                    Id          = Guid.Parse(cols[1]),
                    LogDate     = DateTimeOffset.Parse(cols[2]),
                    IsDriving   = int.Parse(cols[3]) is not 0,
                    Mileage     = int.Parse(cols[4]),
                    Position    = new Position
                    {
                        Latitude    = float.Parse(cols[5], INVARIANT_CULTURE),
                        Longitude   = float.Parse(cols[6], INVARIANT_CULTURE)
                    },
                    Battery     = new Battery
                    {
                        Level       = float.Parse(cols[7], INVARIANT_CULTURE)
                    }
                });
            }
        }

        public Task<IList<VehicleLogEntity>> GetLogs(string vin, DateTime? filterStart, DateTime? filterEnd)
        {
            return Task.Run<IList<VehicleLogEntity>>(() =>
            {
                if (_storage.Count is 0)
                    return new List<VehicleLogEntity>();

                return _storage[vin]
                    .Where(entry => entry.LogDate >= (filterStart ?? DateTimeOffset.MinValue) &&
                                    entry.LogDate < (filterEnd ?? DateTimeOffset.MaxValue))
                    .Select(entry => (VehicleLogEntity)entry)
                    .ToList();
            });
        }

        public Task<Guid> PostLog(string vin, VehicleLogEntity logEntry)
        {
            return Task.Run(() =>
            {
                if (!IsCached(vin)) // New VIN?
                    _storage.Add(vin, new List<DBLogEntity>());

                Guid id = Guid.NewGuid();

                var log = new DBLogEntity
                {
                    Id          = id,
                    LogDate     = DateTimeOffset.UtcNow,
                    IsDriving   = logEntry.IsDriving,
                    Mileage     = logEntry.Mileage,
                    Position    = logEntry.Position,
                    Battery     = logEntry.Battery
                };

                WriteToCache(vin, log);
                WriteToFile(vin, log);

                return id;
            });
        }

        /// <summary>
        /// Writes log entry to db file
        /// </summary>
        /// <param name="vin">Vehicle Identifier Number</param>
        /// <param name="logEntry">The vehicle log entry</param>
        private static void WriteToFile(string vin, DBLogEntity logEntry)
        {
            using StreamWriter fileOut = File.AppendText(DB_FILE_PATH);
            fileOut.WriteLine(string.Join(SEPARATOR_TOKEN,
                vin,
                logEntry.Id.ToString(),
                logEntry.LogDate.ToString("O"),
                logEntry.IsDriving ? "1" : "0",
                logEntry.Mileage.ToString(),
                logEntry.Position.Latitude.ToString(INVARIANT_CULTURE),
                logEntry.Position.Longitude.ToString(INVARIANT_CULTURE),
                logEntry.Battery.Level.ToString(INVARIANT_CULTURE)
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

        #region Scoped Types
        private class DBLogEntity : VehicleLogEntity
        {
            public Guid Id { get; set; }
        }
        #endregion
    }
}
