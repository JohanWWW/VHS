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

        ~FakeVehicleHookApi()
        {
            // Dispose managed resources
            _connection.Dispose();
        }

        public FakeVehicleHookApi(IVehicleSimulatorBackgroundService vehicleSimulatorService)
        {
            _vehicleSimulatorService = vehicleSimulatorService;
            _connection = new SqliteConnection(CONNECTION_STRING);
            if (!File.Exists(DB_FILE))
            {
                _connection.Open();
                CreateDb();
                return;
            }
            _connection.Open();

            foreach (string vin in GetVins())
            {
                async void onPositionUpdated(GeoCoordinate? coord)
                {
                    // If vehicle does not exist, unsubscribe from events
                    if (!await Exists(vin))
                        _vehicleSimulatorService.PositionUpdated -= onPositionUpdated;

                    await UpdateCurrentPosition(vin, coord);
                }
                async void OnDistanceUpdated(DistanceEventArgs args)
                {
                    if (!await Exists(vin))
                        _vehicleSimulatorService.DistanceUpdated -= OnDistanceUpdated;
                    await UpdateMilage(vin, args.TotalDistance);
                }

                _vehicleSimulatorService.PositionUpdated += onPositionUpdated;
                _vehicleSimulatorService.DistanceUpdated += OnDistanceUpdated;
            }
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
            GeoCoordinate? position = null;
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"
select v.Latitude, v.Longitude
from Vehicle as v
where v.Vin = $vin;
";
            DbParameter vinParameter = cmd.CreateParameter();
            vinParameter.ParameterName = "$vin";
            vinParameter.Value = vin;
            cmd.Parameters.Add(vinParameter);

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
    s.IsAlarmActivated
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
                    Mileage = await reader.GetFieldValueAsync<int>(3),
                    Tires = new StatusDBEntity.TirePressure
                    {
                        FrontLeft = await reader.GetFieldValueAsync<float>(4),
                        FrontRight = await reader.GetFieldValueAsync<float>(5),
                        BackLeft = await reader.GetFieldValueAsync<float>(6),
                        BackRight = await reader.GetFieldValueAsync<float>(7)
                    },
                    IsLocked = await reader.GetFieldValueAsync<bool>(8),
                    IsAlarmActivated = await reader.GetFieldValueAsync<bool>(9)
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
insert into State (Latitude, Longitude, Mileage, FrontLeftTire, FrontRightTire, BackLeftTire, BackRightTire, IsLocked, IsAlarmActivated) values (
    $latitude, 
    $longitude, 
    $mileage, 
    $frontLeftTire, 
    $frontRightTire, 
    $backLeftTire, 
    $backRightTire,
    $isLocked,
    $isAlarmActivated
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

            async void onPositionUpdated(GeoCoordinate? coord)
            {
                // If vehicle does not exist, unsubscribe from events
                if (!await Exists(vin))
                    _vehicleSimulatorService.PositionUpdated -= onPositionUpdated;

                await UpdateCurrentPosition(vin, coord);
            }
            async void OnDistanceUpdated(DistanceEventArgs args)
            {
                if (!await Exists(vin))
                    _vehicleSimulatorService.DistanceUpdated -= OnDistanceUpdated;
                await UpdateMilage(vin, args.Distance);
            }

                _vehicleSimulatorService.PositionUpdated += onPositionUpdated;
                _vehicleSimulatorService.DistanceUpdated += OnDistanceUpdated;



            return true;
        }

        public async Task<bool> Exists(string vin)
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText = "select * from Vehicle as v where v.Vin = $vin;";
            cmd.Parameters.Add(CreateDbParameter(cmd, "$vin", vin));
            return await cmd.ExecuteScalarAsync() is not null;
        }

        private IEnumerable<string> GetVins()
        {
            DbCommand getVinsCmd = _connection.CreateCommand();
            getVinsCmd.CommandText = "select Vin from Vehicle";

            using var reader = getVinsCmd.ExecuteReader();

            while (reader.Read())
                yield return reader.GetFieldValue<string>(0);
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
            DbCommand getStateIdCmd = _connection.CreateCommand();
            getStateIdCmd.CommandText = "select StateId from Vehicle where Vin = $vin";
            getStateIdCmd.Parameters.Add(CreateDbParameter(getStateIdCmd, "$vin", vin));

            stateId = (long)await getStateIdCmd.ExecuteScalarAsync();

            DbCommand updateMilCmd = _connection.CreateCommand();
            updateMilCmd.CommandText = "update State set Mileage = $mileage where Id = $stateId";
            updateMilCmd.Parameters.Add(CreateDbParameter(updateMilCmd, "$mileage", distance));
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
    Mileage             integer                     not null,
    FrontLeftTire       real                        not null,
    FrontRightTire      real                        not null,
    BackLeftTire        real                        not null,
    BackRightTire       real                        not null,
    IsLocked            int                         not null,
    IsAlarmActivated    int                         not null
);
";
            _ = cmd.ExecuteNonQuery();
        }

        #region Scoped Types
        private class StatusDBEntity
        {
            public string Vin { get; set; }
            public GeoCoordinate Position { get; set; }
            public int Mileage { get; set; }
            public TirePressure Tires { get; set; }
            public bool IsLocked { get; set; }
            public bool IsAlarmActivated { get; set; }

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
