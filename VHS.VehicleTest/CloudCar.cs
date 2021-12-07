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
        private int _mileage = 50;
        public int Mileage => _mileage;
        private Position _position;
        public Position Position => _position;

        private Battery _battery = new Battery
        {
            Level = 1.0f
        };
        public Battery Battery => _battery;
        
        

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
        private double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public void DriveSimulator()
        {
           DateTime starttime = DateTime.Now;
            DateTime stopTime = starttime.AddMinutes(GetRandomNumber(1, 300));
            _mileage = (int)(_mileage + GetRandomNumber(1, 500));

            Battery b = new Battery
            {
                Level = (float)(_battery.Level - GetRandomNumber(0, 0.3))
            };
            _battery = b;

            _position.Latitude = (float)GetRandomNumber(55.37514, 67.85572);
            _position.Longitude = (float)GetRandomNumber(11.1712, 23.15645);

        }
    }
}
