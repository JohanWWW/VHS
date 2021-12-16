using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.Extensions;
using VHS.Backend.HostedServices.Interfaces;
using VHS.Backend.Repositories.Interfaces;
using VHS.Utility.Mapping;
using VHS.Utility.Types;

namespace VHS.Backend.Apis
{
    /// <summary>
    /// Represents a direct communication with a physical car. It also represents communication with database.
    /// </summary>
    public class FakeVehicleHookApi : IVehicleClientApi
    {
        private const string CONNECTION_STRING          = "Data Source=fakevehicles.db";
        private const string DB_FILE                    = "fakevehicles.db";

        private readonly DbConnection _connection;
        private readonly IVehicleSimulatorBackgroundService _vehicleSimulatorService;
        private readonly IDriveLogRepository _driveLogRepository;

        ~FakeVehicleHookApi()
        {
            // Dispose managed resources
            _connection.Dispose();
        }

        public FakeVehicleHookApi(IVehicleSimulatorBackgroundService vehicleSimulatorService, IDriveLogRepository driveLogRepository)
        {
            _vehicleSimulatorService = vehicleSimulatorService;
            _driveLogRepository = driveLogRepository;

            _connection = new SqliteConnection(CONNECTION_STRING);

            if (!File.Exists(DB_FILE))
            {
                _connection.Open();
                CreateDb();
                return;
            }
            _connection.Open();

            foreach (string vin in GetVins())
                SubscribeOnSimulatorEvents(vin);
        }

        public async Task<bool> Beep(string vin)
        {
            if (!await Exists(vin))
                return false;

            Console.Beep();
            return true;
        }

        public async Task<bool> Blink(string vin)
        {
            return await Exists(vin);
        }

        public async Task<GeoCoordinate?> GetCurrentPosition(string vin)
        {
            long stateId;
            GeoCoordinate? position = null;

            stateId = (long)await _connection
                .CreateCommand("select StateId from Vehicle where Vin = $vin")
                .AddParameter("$vin", vin)
                .ExecuteScalarAsync();

            using var reader = await _connection
                .CreateCommand("select s.Latitude, s.Longitude from State as s where s.Id = $stateId")
                .AddParameter("$stateId", stateId)
                .ExecuteReaderAsync();
            
            if (reader.Read())
            {
                position = new GeoCoordinate
                {
                    Latitude = await reader.GetFieldValueAsync<double>(0),
                    Longitude = await reader.GetFieldValueAsync<double>(1)
                };
            }

            return position;
        }

        public async Task<VehicleStatusResponse> GetStatus(string vin)
        {
            StatusDBEntity dbEntity = null;
            VehicleStatusResponse vehicleStatusResponse;

            using var reader = await _connection
                .CreateCommand(
@"
select
    v.Vin,
    s.Latitude,
    s.Longitude,
    s.Mileage,
    s.FrontLeftTire,
    s.FrontRightTire,
    s.BackLeftTire,
    s.BackRightTire,
    s.IsLocked,
    s.IsAlarmActivated,
    s.IsDriveInProgress,
    s.BatteryLevel
from State as s
join Vehicle as v on s.Id = v.StateId
where v.Vin = $vin;
")
                .AddParameter("$vin", vin)
                .ExecuteReaderAsync();

            if (reader.Read())
            {
                dbEntity = new StatusDBEntity
                {
                    Vin                 = await reader.GetFieldValueAsync<string>(0),
                    Position            = new GeoCoordinate
                    {
                        Latitude            = await reader.GetFieldValueAsync<double>(1),
                        Longitude           = await reader.GetFieldValueAsync<double>(2)
                    },
                    Mileage             = await reader.GetFieldValueAsync<double>(3),
                    Tires               = new StatusDBEntity.TirePressure
                    {
                        FrontLeft           = await reader.GetFieldValueAsync<float>(4),
                        FrontRight          = await reader.GetFieldValueAsync<float>(5),
                        BackLeft            = await reader.GetFieldValueAsync<float>(6),
                        BackRight           = await reader.GetFieldValueAsync<float>(7)
                    },
                    IsLocked            = await reader.GetFieldValueAsync<bool>(8),
                    IsAlarmActivated    = await reader.GetFieldValueAsync<bool>(9),
                    IsDriveInProgress   = await reader.GetFieldValueAsync<bool>(10),
                    BatteryLevel        = await reader.GetFieldValueAsync<double>(11)
                };
            }

            if (dbEntity is null)
                return null;

            vehicleStatusResponse = AutoMapper.Map<VehicleStatusResponse>(dbEntity);
            vehicleStatusResponse.ServerDateTime = DateTimeOffset.Now;

            return vehicleStatusResponse;
        }

        public async Task<bool> AddVehicle(string vin)
        {
            if (await Exists(vin))
                return false;

            long stateId;
            _ = await _connection.CreateCommand(
@"
insert into State (Latitude, Longitude, Mileage, FrontLeftTire, FrontRightTire, BackLeftTire, BackRightTire, IsLocked, IsAlarmActivated, IsDriveInProgress, BatteryLevel) values (
    $latitude, 
    $longitude, 
    $mileage, 
    $frontLeftTire, 
    $frontRightTire, 
    $backLeftTire, 
    $backRightTire,
    $isLocked,
    $isAlarmActivated,
    $isDriveInProgress,
    $batteryLevel
);
")
                .AddParameter("$latitude", .0f)
                .AddParameter("$longitude", .0f)
                .AddParameter("$mileage", .0f)
                .AddParameter("$frontLeftTire", .0f)
                .AddParameter("$frontRightTire", .0f)
                .AddParameter("$backLeftTire", .0f)
                .AddParameter("$backRightTire", .0f)
                .AddParameter("$isLocked", false)
                .AddParameter("$isAlarmActivated", false)
                .AddParameter("$isDriveInProgress", false)
                .AddParameter("$batteryLevel", 1d)
                .ExecuteNonQueryAsync();

            stateId = (long)await _connection
                .CreateCommand("select last_insert_rowid()")
                .ExecuteScalarAsync();

            _ = await _connection
                .CreateCommand("insert into Vehicle values ($vin, $stateId)")
                .AddParameter("$vin", vin)
                .AddParameter("$stateId", stateId)
                .ExecuteNonQueryAsync();

            SubscribeOnSimulatorEvents(vin);

            return true;
        }

        public async Task<bool> Exists(string vin)
        {
            return await _connection
                .CreateCommand("select 1 from Vehicle as v where v.Vin = $vin")
                .AddParameter("$vin", vin)
                .ExecuteScalarAsync() is not null;
        }

        public IEnumerable<string> GetVins()
        {
            using var reader = _connection
                .CreateCommand("select Vin from Vehicle")
                .ExecuteReader();

            while (reader.Read())
                yield return reader.GetFieldValue<string>(0);
        }

        private void SubscribeOnSimulatorEvents(string vin)
        {
            const double BATTERY_CONSUMPTION_FACTOR = 0.0025d;

            // Local functions
            async void onPositionUpdated(GeoCoordinate? coord)
            {
                bool isDriving = true;
                double distance;
                double consumedBattery;
                GeoCoordinate? previousPosition;

                // If vehicle does not exist, unsubscribe from events
                if (!await Exists(vin))
                    _vehicleSimulatorService.PositionUpdated -= onPositionUpdated;

                var status = await GetStatus(vin);
                if (!status.IsDriveInProgress)
                    return;

                previousPosition = status.Position;
                distance = GeoCoordinate.GetMetricDistance((GeoCoordinate)previousPosition, (GeoCoordinate)coord);
                if (distance > 1d)
                    isDriving = false;
                consumedBattery = distance * BATTERY_CONSUMPTION_FACTOR;

                await UpdateCurrentPosition(vin, coord);
                await UpdateMilage(vin, distance);
                await UpdateBatteryLevel(vin, consumedBattery);

                _ = await _driveLogRepository.PostLog(vin, new Entities.VehicleLogEntity
                {
                    IsDriving = isDriving,
                    Position = (GeoCoordinate)coord,
                    Mileage = status.Mileage + distance,
                    Battery = new Battery
                    {
                        Level = status.BatteryLevel - consumedBattery
                    }
                });
            }

            _vehicleSimulatorService.PositionUpdated += onPositionUpdated;
        }

        private async Task UpdateCurrentPosition(string vin, GeoCoordinate? coord)
        {
            long stateId;

            stateId = (long)await _connection
                .CreateCommand("select StateId from Vehicle where Vin = $vin")
                .AddParameter("$vin", vin)
                .ExecuteScalarAsync();

            _ = await _connection
                .CreateCommand("update State set Latitude = $lat, Longitude = $lon where Id = $stateId")
                .AddParameter("$lat", coord.Value.Latitude)
                .AddParameter("$lon", coord.Value.Longitude)
                .AddParameter("$stateId", stateId)
                .ExecuteNonQueryAsync();
        }

        private async Task UpdateMilage(string vin, double distance)
        {
            long stateId;
            double currentMileage;

            stateId = (long)await _connection
                .CreateCommand("select StateId from Vehicle where Vin = $vin")
                .AddParameter("$vin", vin)
                .ExecuteScalarAsync();

            currentMileage = (double)await _connection
                .CreateCommand("select Mileage from State where Id = $stateId")
                .AddParameter("$stateId", stateId)
                .ExecuteScalarAsync();

            _ = await _connection
                .CreateCommand("update State set Mileage = $mileage where Id = $stateId")
                .AddParameter("$mileage", distance + currentMileage)
                .AddParameter("$stateId", stateId)
                .ExecuteNonQueryAsync();
        }

        private async Task UpdateBatteryLevel(string vin, double level)
        {
            long stateId;
            double currentBatteryLevel;

            stateId = (long)await _connection
                .CreateCommand("select StateId from Vehicle where Vin = $vin")
                .AddParameter("$vin", vin)
                .ExecuteScalarAsync();

            currentBatteryLevel = (double)await _connection
                .CreateCommand("select BatteryLevel from State where Id = $stateId")
                .AddParameter("$stateId", stateId)
                .ExecuteScalarAsync();

            _ = await _connection
                .CreateCommand("update State set BatteryLevel = $batteryLevel where Id = $stateId")
                .AddParameter("$batteryLevel", currentBatteryLevel - level)
                .AddParameter("$stateId", stateId)
                .ExecuteNonQueryAsync();
        }

        private void CreateDb()
        {
            CreateStateTable();
            CreateVehicleTable();
        }

        private void CreateVehicleTable()
        {
            _ = _connection.CreateCommand(
@"

create table Vehicle (
    Vin     text        primary key not null,
    StateId integer     not null,
    foreign key(StateId) references State(Id)
);
")
                .ExecuteNonQuery();
        }

        private void CreateStateTable()
        {
            _ = _connection.CreateCommand(
@"
create table State (
    Id                  integer                     primary key                 autoincrement,
    Latitude            real                        not null,
    Longitude           real                        not null,
    Mileage             real                        not null,
    FrontLeftTire       real                        not null,
    FrontRightTire      real                        not null,
    BackLeftTire        real                        not null,
    BackRightTire       real                        not null,
    IsLocked            integer                     not null,
    IsAlarmActivated    integer                     not null,
    IsDriveInProgress   integer                     not null,
    BatteryLevel        real                        not null
);
")
                .ExecuteNonQuery();
        }

        #region Scoped Types
        private class StatusDBEntity
        {
            public string Vin { get; set; }
            public GeoCoordinate Position { get; set; }
            public double Mileage { get; set; }
            public TirePressure Tires { get; set; }
            public bool IsLocked { get; set; }
            public bool IsAlarmActivated { get; set; }
            public bool IsDriveInProgress { get; set; }
            public double BatteryLevel { get; set; }

            public class TirePressure
            {
                public float FrontLeft { get; set; }
                public float FrontRight { get; set; }
                public float BackLeft { get; set; }
                public float BackRight { get; set; }
            }
        }
        #endregion
    }
}
