using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Backend.HostedServices.Interfaces;
using VHS.Backend.Repositories.Interfaces;
using VHS.Utility.Mapping;
using VHS.Utility.Types;
using VHS.VehicleTest;

namespace VHS.Backend.Apis
{
    /// <summary>
    /// Represents a direct communication with a physical car. It also represents communication with database.
    /// </summary>
    public class FakeVehicleHookApi : IVehicleClientApi
    {
        private const string CONNECTION_STRING = "Data Source=fakevehicles.db";
        private const string DB_FILE = "fakevehicles.db";

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

            DbCommand getStateId = _connection.CreateCommand();
            getStateId.CommandText = "select StateId from Vehicle where Vin = $vin";
            getStateId.Parameters.Add(CreateDbParameter(getStateId, "$vin", vin));
            stateId = (long)await getStateId.ExecuteScalarAsync();

            GeoCoordinate? position = null;
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"
select s.Latitude, s.Longitude
from State as s
where s.Id = $stateId;
";

            cmd.Parameters.Add(CreateDbParameter(cmd, "$stateId", stateId));
            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                position = new GeoCoordinate
                {
                    Latitude = await reader.GetFieldValueAsync<float>(0),
                    Longitude = await reader.GetFieldValueAsync<float>(1)
                };
            }

            return position;
        }

        public async Task<VehicleStatusResponse> GetStatus(string vin)
        {
            StatusDBEntity dbEntity = null;
            VehicleStatusResponse vehicleStatusResponse;

            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
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
    s.IsDriveInProgress
from State as s
join Vehicle as v on s.Id = v.StateId
where v.Vin = $vin;
";
            DbParameter vinParameter = cmd.CreateParameter();
            vinParameter.ParameterName = "$vin";
            vinParameter.Value = vin;
            cmd.Parameters.Add(vinParameter);

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                dbEntity = new StatusDBEntity
                {
                    Vin = await reader.GetFieldValueAsync<string>(0),
                    Position = new GeoCoordinate
                    {
                        Latitude = await reader.GetFieldValueAsync<float>(1),
                        Longitude = await reader.GetFieldValueAsync<float>(2)
                    },
                    Mileage = await reader.GetFieldValueAsync<double>(3),
                    Tires = new StatusDBEntity.TirePressure
                    {
                        FrontLeft = await reader.GetFieldValueAsync<float>(4),
                        FrontRight = await reader.GetFieldValueAsync<float>(5),
                        BackLeft = await reader.GetFieldValueAsync<float>(6),
                        BackRight = await reader.GetFieldValueAsync<float>(7)
                    },
                    IsLocked = await reader.GetFieldValueAsync<bool>(8),
                    IsAlarmActivated = await reader.GetFieldValueAsync<bool>(9),
                    IsDriveInProgress = await reader.GetFieldValueAsync<bool>(10)
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
            DbCommand addStateCmd = _connection.CreateCommand();
            addStateCmd.CommandText =
@"
insert into State (Latitude, Longitude, Mileage, FrontLeftTire, FrontRightTire, BackLeftTire, BackRightTire, IsLocked, IsAlarmActivated, IsDriveInProgress) values (
    $latitude, 
    $longitude, 
    $mileage, 
    $frontLeftTire, 
    $frontRightTire, 
    $backLeftTire, 
    $backRightTire,
    $isLocked,
    $isAlarmActivated,
    $isDriveInProgress
);
";

            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$latitude", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$longitude", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$mileage", 0));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$frontLeftTire", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$frontRightTire", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$backLeftTire", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$backRightTire", .0f));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$isLocked", false));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$isAlarmActivated", false));
            addStateCmd.Parameters.Add(CreateDbParameter(addStateCmd, "$isDriveInProgress", false));
            _ = await addStateCmd.ExecuteNonQueryAsync();

            DbCommand lastInsertIdCmd = _connection.CreateCommand();
            lastInsertIdCmd.CommandText = "select last_insert_rowid()";
            stateId = (long)await lastInsertIdCmd.ExecuteScalarAsync();

            DbCommand addVehicleCmd = _connection.CreateCommand();
            addVehicleCmd.CommandText =
@"
insert into Vehicle values (
    $vin,
    $stateId
);
";

            addVehicleCmd.Parameters.Add(CreateDbParameter(addVehicleCmd, "$vin", vin));
            addVehicleCmd.Parameters.Add(CreateDbParameter(addVehicleCmd, "$stateId", stateId));
            _ = await addVehicleCmd.ExecuteNonQueryAsync();

            SubscribeOnSimulatorEvents(vin);

            return true;
        }

        public async Task<bool> Exists(string vin)
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText = "select 1 from Vehicle as v where v.Vin = $vin;";
            cmd.Parameters.Add(CreateDbParameter(cmd, "$vin", vin));
            return await cmd.ExecuteScalarAsync() is not null;
        }

        public IEnumerable<string> GetVins()
        {
            DbCommand getVinsCmd = _connection.CreateCommand();
            getVinsCmd.CommandText = "select Vin from Vehicle";

            using var reader = getVinsCmd.ExecuteReader();

            while (reader.Read())
                yield return reader.GetFieldValue<string>(0);
        }

        private void SubscribeOnSimulatorEvents(string vin)
        {
            // Local functions
            async void onPositionUpdated(GeoCoordinate? coord)
            {
                // If vehicle does not exist, unsubscribe from events
                if (!await Exists(vin))
                    _vehicleSimulatorService.PositionUpdated -= onPositionUpdated;

                var status = await GetStatus(vin);
                if (!status.IsDriveInProgress)
                    return;

                GeoCoordinate? previousPosition = status.Position;
                double distance = GeoCoordinate.GetMetricDistance((GeoCoordinate)previousPosition, (GeoCoordinate)coord);

                await UpdateCurrentPosition(vin, coord);
                await UpdateMilage(vin, distance);

                _ = await _driveLogRepository.PostLog(vin, new Entities.VehicleLogEntity
                {
                    IsDriving = true,
                    Position = (GeoCoordinate)coord,
                    Mileage = status.Mileage + distance
                });
            }

            _vehicleSimulatorService.PositionUpdated += onPositionUpdated;
        }

        private async Task UpdateCurrentPosition(string vin, GeoCoordinate? coord)
        {
            long stateId;

            DbCommand getStateIdCmd = _connection.CreateCommand();
            getStateIdCmd.CommandText = "select StateId from Vehicle where Vin = $vin";
            getStateIdCmd.Parameters.Add(CreateDbParameter(getStateIdCmd, "$vin", vin));

            stateId = (long)await getStateIdCmd.ExecuteScalarAsync();

            DbCommand updatePosCmd = _connection.CreateCommand();
            updatePosCmd.CommandText = "update State set Latitude = $lat, Longitude = $lon where Id = $stateId";
            updatePosCmd.Parameters.Add(CreateDbParameter(updatePosCmd, "$lat", ((GeoCoordinate)coord).Latitude));
            updatePosCmd.Parameters.Add(CreateDbParameter(updatePosCmd, "$lon", ((GeoCoordinate)coord).Longitude));
            updatePosCmd.Parameters.Add(CreateDbParameter(updatePosCmd, "$stateId", stateId));
            _ = await updatePosCmd.ExecuteNonQueryAsync();
        }

        private async Task UpdateMilage(string vin, double distance)
        {
            long stateId;
            double currentMileage;

            DbCommand getStateIdCmd = _connection.CreateCommand();
            getStateIdCmd.CommandText = "select StateId from Vehicle where Vin = $vin";
            getStateIdCmd.Parameters.Add(CreateDbParameter(getStateIdCmd, "$vin", vin));
            stateId = (long)await getStateIdCmd.ExecuteScalarAsync();

            DbCommand getMilCmd = _connection.CreateCommand();
            getMilCmd.CommandText = "select Mileage from State where Id = $stateId";
            getMilCmd.Parameters.Add(CreateDbParameter(getMilCmd, "$stateId", stateId));
            currentMileage = (double)await getMilCmd.ExecuteScalarAsync();

            DbCommand updateMilCmd = _connection.CreateCommand();
            updateMilCmd.CommandText = "update State set Mileage = $mileage where Id = $stateId";
            updateMilCmd.Parameters.Add(CreateDbParameter(updateMilCmd, "$mileage", distance + currentMileage));
            updateMilCmd.Parameters.Add(CreateDbParameter(updateMilCmd, "$stateId", stateId));
            _ = await updateMilCmd.ExecuteNonQueryAsync();
        }

        private static DbParameter CreateDbParameter(DbCommand cmd, string key, object value)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = key[0] is not '$' ? $"${key}" : key;
            parameter.Value = value;
            return parameter;
        }

        private void CreateDb()
        {
            CreateStateTable();
            CreateVehicleTable();
        }

        private void CreateVehicleTable()
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"

create table Vehicle (
    Vin     text        primary key not null,
    StateId integer     not null,
    foreign key(StateId) references State(Id)
);
";
            _ = cmd.ExecuteNonQuery();
        }

        private void CreateStateTable()
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"
create table State (
    Id                  INTEGER    PRIMARY KEY     AUTOINCREMENT,
    Latitude            real                        not null,
    Longitude           real                        not null,
    Mileage             real                        not null,
    FrontLeftTire       real                        not null,
    FrontRightTire      real                        not null,
    BackLeftTire        real                        not null,
    BackRightTire       real                        not null,
    IsLocked            integer                     not null,
    IsAlarmActivated    integer                     not null,
    IsDriveInProgress   integer                     not null
);
";
            _ = cmd.ExecuteNonQuery();
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
