using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VHS.Backend.HostedServices.Interfaces;
using VHS.Utility.Types;

namespace VHS.Backend.HostedServices
{
    public class VehicleSimulatorBackgroundService : IVehicleSimulatorBackgroundService
    {
        private const string ROUTE_FILE_PATH = "E6_rutt.txt";

        //private const double BATTERY_CAPACITY = 100d;
        //private const double POWER_PER_10KM = 2.5d;
        //private const double POWER_PER_KM = POWER_PER_10KM * 0.1;

        private const double POWER_CONSUMPTION_FACTOR = 0.0025d;

        private static readonly IFormatProvider _culture = System.Globalization.CultureInfo.InvariantCulture;

        private DateTime _simulationStart;

        private GeoCoordinate? _currentPosition = null;
        public GeoCoordinate? Position
        {
            get => _currentPosition;
            private set
            {
                if (value != _currentPosition)
                {
                    _currentPosition = value;
                    PositionUpdated?.Invoke(value);
                }
            }
        }

        private double _distance = 0;
        public double Distance
        {
            get => _distance;
            private set
            {
                if (value != _distance)
                {
                    _distance = value;
                    DistanceUpdated?.Invoke(new DistanceEventArgs(_distance, _totalDistance));
                }
            }
        }

        private double _totalDistance = 0;
        public double TotalDistance
        {
            get => _totalDistance;
            private set
            {
                if (value != _totalDistance)
                {
                    _totalDistance = value;
                    DistanceUpdated?.Invoke(new DistanceEventArgs(_distance, _totalDistance));
                }
            }
        }

        private double _batteryLevel = 1d;
        public double BatteryLevel
        {
            get => _batteryLevel;
            private set
            {
                if (value != _batteryLevel)
                {
                    _batteryLevel = value;
                    // TODO: 
                }
            }
        }

        public event PositionUpdatedEventHandler PositionUpdated;
        public event DistanceUpdatedEventHandler DistanceUpdated;

        public VehicleSimulatorBackgroundService()
        {
        }

        public Task StartAsync()
        {
            
            DateTime startts;
            IEnumerable<GeoCoordinate> coords;

            startts = DateTime.Now;
            coords = GetCoordinates();

            // 
            var speeds = new List<double>();

            return Task.Run(
                action: async () =>
                {
                    _simulationStart = DateTime.Now;

                    foreach (var coord in coords)
                    {
                        if (Position is null)
                            Position = coord;

                        DateTime ts = DateTime.Now;
                        double d = GeoCoordinate.GetMetricDistance((GeoCoordinate)Position, coord);

                        TotalDistance += d;
                        Distance = d;
                        BatteryLevel -= Distance * POWER_CONSUMPTION_FACTOR;
                        Position = coord;

                        speeds.Add(d / (ts - startts).TotalHours);

                        StringBuilder sb = new();
                        sb.Append("Hastighet:\t\t\t\t\t").Append(Math.Round(d / (ts - startts).TotalHours, 1)).Append(" km/h").AppendLine();
                        sb.Append("Batterinivå:\t\t\t\t").Append(Math.Round(BatteryLevel * 100, 1)).Append('%').AppendLine();
                        sb.Append("Körsträcka mellan punkter:\t").Append(Math.Round(Distance * 1000, 1)).Append(" m").AppendLine();
                        sb.Append("Total körsträcka:\t\t\t").Append(Math.Round(TotalDistance, 1)).Append(" km").AppendLine();
                        sb.Append("Förfluten tid:\t\t\t\t").Append(ts - _simulationStart).Append(" (").Append(Math.Round((ts - _simulationStart).TotalSeconds, 1)).Append(" s)").AppendLine();
                        sb.Append("Snitthastighet:\t\t\t\t").Append(speeds.Average()).AppendLine(" km/h\n");

                        System.Diagnostics.Debug.Write(sb.ToString());

                        startts = ts;

                        //await Task.Delay(8800);
                        await Task.Delay(10_084);
                    }
                }, 
                cancellationToken: CancellationToken.None
            );
        }

        private static double Lerp(double start, double end, double t) =>
            start + (end - start) * t;

        private static GeoCoordinate Lerp(GeoCoordinate start, GeoCoordinate end, double t) => new()
        {
            Latitude = Lerp(start.Latitude, end.Latitude, t),
            Longitude = Lerp(start.Longitude, end.Longitude, t)
        };

        private static IEnumerable<GeoCoordinate> GetCoordinates()
        {
            using IEnumerator<string> fileEnumerator = File.ReadLines(ROUTE_FILE_PATH).GetEnumerator();

            if (!fileEnumerator.MoveNext())
                yield break;

            while (fileEnumerator.MoveNext())
            {
                string[] cells = fileEnumerator.Current.Split('\t', StringSplitOptions.RemoveEmptyEntries);

                yield return new GeoCoordinate
                {
                    Latitude    = double.Parse(cells[0], _culture),
                    Longitude   = double.Parse(cells[1], _culture)
                };
            }
        }
    }
}
