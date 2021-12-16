using System;
using VHS.Utility.Types;

namespace VHS.Backend.Apis.Responses
{
    public class VehicleStatusResponse
    {
        public DateTimeOffset ServerDateTime { get; set; }
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
}
