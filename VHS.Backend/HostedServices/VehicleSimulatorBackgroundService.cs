using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VHS.Backend.HostedServices.Interfaces;
using VHS.Utility.Types;

namespace VHS.Backend.HostedServices
{
    public class VehicleSimulatorBackgroundService : IVehicleSimulatorBackgroundService
    {
        private const string ROUTE_FILE_PATH = "E6_rutt.txt";
        private const int RESOLUTION = 1000;

        private static IFormatProvider _culture = System.Globalization.CultureInfo.InvariantCulture;

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
        public event PositionUpdatedEventHandler PositionUpdated;
        public event DistanceUpdatedEventHandler DistanceUpdated;

        public VehicleSimulatorBackgroundService()
        {
        }

        public Task StartAsync()
        {
            
            DateTime startdt;
            IEnumerable<GeoCoordinate> coords;

            startdt = DateTime.Now;
            coords = GetCoordinates();

            return Task.Run(
                action: async () =>
                {
                    foreach (var coord in coords)
                    {
                        if (Position is not null)
                        {
                            // Inserts additional points between existing points for smoothness
                            for (int i = 0; i < RESOLUTION; i++)
                            {
                                float t = (float)i / RESOLUTION;
                                GeoCoordinate transition = Lerp((GeoCoordinate)Position, coord, t);
                                double d = GeoCoordinate.GetMetricDistance((GeoCoordinate)Position, coord);
                                await Task.Delay(10);

                                TotalDistance += d;
                                Distance = d;
                                Position = coord;
                            }
                        }
                        else
                            Position = coord;
                    }
                }, 
                cancellationToken: CancellationToken.None
            );
        }

        private static float Lerp(float start, float end, float t) =>
            start + (end - start) * t;

        private static GeoCoordinate Lerp(GeoCoordinate start, GeoCoordinate end, float t) => new()
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
                    Latitude    = float.Parse(cells[0], _culture),
                    Longitude   = float.Parse(cells[1], _culture)
                };
            }
        }
    }
}
