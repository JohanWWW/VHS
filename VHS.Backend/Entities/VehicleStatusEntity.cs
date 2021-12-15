using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Utility.Types;
using VHS.VehicleTest;

namespace VHS.Backend.Entities
{
    public class VehicleStatusEntity
    {
        public DateTimeOffset ServerDateTime { get; set; }
        public GeoCoordinate Position { get; set; }
        public int Mileage { get; set; }
        public TirePressure Tires { get; set; }
        public bool IsLocked { get; set; }
        public bool IsAlarmActivated { get; set; }
        public bool IsDriving { get; set; }

        public class TirePressure
        {
            public float FrontLeft { get; set; }
            public float FrontRight { get; set; }
            public float BackLeft { get; set; }
            public float BackRight { get; set; }
        }
    }
}
