using System;
using VHS.Utility.Types;
using VHS.VehicleTest;

namespace VHS.Backend.Entities
{
    public class VehicleLogEntity
    {
        public DateTimeOffset LogDate { get; set; }
        public bool IsDriving { get; set; }
        public double Mileage { get; set; }
        public GeoCoordinate Position { get; set; }
        public Battery Battery { get; set; }
    }
}
