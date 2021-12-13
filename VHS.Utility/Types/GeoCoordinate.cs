using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace VHS.Utility.Types
{
    public struct GeoCoordinate
    {
        private const int EARTH_RADIUS      = 6367;
        private const double DEG_TO_RAD     = PI / 180.0;

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public override bool Equals(object obj)
        {
            GeoCoordinate geo;

            if (obj is not GeoCoordinate)
                return false;

            geo = (GeoCoordinate)obj;
            return geo.Latitude == Latitude && geo.Longitude == Longitude;
        }

        public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

        public override string ToString() => 
            $"{{{Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)};{{{Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}}}";

        public static double GetMetricDistance(GeoCoordinate a, GeoCoordinate b) =>
            Haversine(a.Latitude, a.Longitude, b.Latitude, b.Longitude);

        public static bool operator ==(GeoCoordinate a, GeoCoordinate b) => a.Equals(b);

        public static bool operator !=(GeoCoordinate a, GeoCoordinate b) => !a.Equals(b);

        private static double Haversine(float lat1, float lon1, float lat2, float lon2)
        {
            double dlon = (lon2 - lon1) * DEG_TO_RAD;
            double dlat = (lat2 - lat1) * DEG_TO_RAD;

            double a = Pow(Sin(dlat / 2.0), 2) + Pow(Cos(lat1 * DEG_TO_RAD), 2) * Pow(Sin(dlon / 2.0), 2);
            double c = 2 * Atan2(Sqrt(a), Sqrt(1 - a));
            double d = EARTH_RADIUS * c;

            return d;
        }
    }
}
