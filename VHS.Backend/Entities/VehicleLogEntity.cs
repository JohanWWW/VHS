using System;
using VHS.VehicleTest;

namespace VHS.Backend.Entities
{
    public class VehicleLogEntity
    {
        public DateTime LogDate { get; set; }
        public bool IsDriving { get; set; }
        public int Mileage { get; set; }
        public Position Position { get; set; }
        public Battery Battery { get; set; }
    }
}
