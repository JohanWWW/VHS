using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.VehicleTest
{
    /// <summary>
    /// Represents a car
    /// </summary>
    public interface IVehicle
    {
        bool IsLocked { get; }
        bool IsAlarmActivated { get; }
        bool IsDriving { get; }
        int Mileage { get; }
        Position Position { get; }
        Battery Battery { get; }

        bool Beep();
        bool Blink();
        float GetPressure(int index);
    }

    public struct Position
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public struct Battery
    {
        public float Level { get; set; }
    }

    public enum PressureUnit
    {
        Bar,
        Kpa
    }
}
