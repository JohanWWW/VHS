using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VHS.VehicleTest
{
    public class CloudCar : IVehicle
    {
        private readonly float[] _tirePressures;

        public bool IsLocked => false;
        public bool IsAlarmActivated => false;
        public bool IsDriving => false;
        public int Mileage => 50;

        public Position Position => new()
        {
            Longitude = 58.058832f,
            Latitude = 11.787748f
        };

        public Battery Battery => new()
        {
            Level = 0.8f
        };

        public CloudCar()
        {
            _tirePressures = new float[] { 2.8f, 2.8f, 2.8f, 2.79f };
        }

        public bool Beep()
        {
            Console.Beep();
            return true;
        }

        public bool Blink() => true;

        public float GetPressure(int index)
        {
            return _tirePressures[index];
        }
    }
}
