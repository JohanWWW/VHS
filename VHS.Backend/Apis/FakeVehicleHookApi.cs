using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VHS.Backend.Apis.Interfaces;
using VHS.Backend.Apis.Responses;
using VHS.Utility.Mapping;
using VHS.Utility.Types;
using VHS.VehicleTest;

namespace VHS.Backend.Apis
{
    /// <summary>
    /// Represents a fake direct communication with a car
    /// </summary>
    public class FakeVehicleHookApi : IVehicleClientApi
    {
        private const string CONNECTION_STRING = "Data Source=fakevehicles.db";
        private const string DB_FILE = "fakevehicles.db";

        private readonly DbConnection _connection;

        ~FakeVehicleHookApi()
        {
            // Dispose managed resources
            _connection.Dispose();
        }

        public FakeVehicleHookApi()
        {
            _connection = new SqliteConnection(CONNECTION_STRING);
            if (!File.Exists(DB_FILE))
            {
                _connection.Open();
                CreateDb();
                return;
            }
            _connection.Open();
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
    v.Latitude,
    v.Longitude,
    v.Mileage,
    v.FrontLeftTire,
    v.FrontRightTire,
    v.BackLeftTire,
    v.BackRightTire,
    v.IsLocked,
    v.IsAlarmActivated
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

            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"
insert into Vehicle
values (
    $vin, 
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
            

            cmd.Parameters.Add(CreateDbParameter(cmd, "$vin", vin));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$latitude", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$longitude", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$mileage", 0));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$frontLeftTire", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$frontRightTire", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$backLeftTire", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$backRightTire", .0f));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$isLocked", false));
            cmd.Parameters.Add(CreateDbParameter(cmd, "$isAlarmActivated", false));

            _ = await cmd.ExecuteNonQueryAsync();

            return true;
        }

        private async Task<bool> Exists(string vin)
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText = "select * from Vehicle as v where v.Vin = $vin;";
            cmd.Parameters.Add(CreateDbParameter(cmd, "$vin", vin));
            return await cmd.ExecuteScalarAsync() is not null;
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
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandText =
@"
create table Vehicle (
    Vin                 text        primary key     not null,
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
