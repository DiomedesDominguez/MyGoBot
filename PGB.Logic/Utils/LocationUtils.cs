namespace PGB.Logic.Utils
{
    using System;
    using System.Device.Location;

    public static class LocationUtils
    {
        #region Methods and other members

        public static double CalculateDistanceInMeters(double sourceLat, double sourceLng, double destLat,
            double destLng)
        {
            return new GeoCoordinate(sourceLat, sourceLng).GetDistanceTo(new GeoCoordinate(destLat, destLng));
        }

        public static double CalculateDistanceInMeters(GeoCoordinate sourceLocation, GeoCoordinate destinationLocation)
        {
            return CalculateDistanceInMeters(sourceLocation.Latitude, sourceLocation.Longitude,
                destinationLocation.Latitude, destinationLocation.Longitude);
        }

        public static GeoCoordinate CreateWaypoint(GeoCoordinate sourceLocation, double distanceInMeters,
            double bearingDegrees)
        {
            var num1 = distanceInMeters/1000.0/6371.0;
            var rad1 = ToRad(bearingDegrees);
            var rad2 = ToRad(sourceLocation.Latitude);
            var rad3 = ToRad(sourceLocation.Longitude);
            var num2 = Math.Asin(Math.Sin(rad2)*Math.Cos(num1) + Math.Cos(rad2)*Math.Sin(num1)*Math.Cos(rad1));
            var num3 = Math.Atan2(Math.Sin(rad1)*Math.Sin(num1)*Math.Cos(rad2),
                Math.Cos(num1) - Math.Sin(rad2)*Math.Sin(num2));
            var radians = (rad3 + num3 + 3.0*Math.PI)%(2.0*Math.PI) - Math.PI;
            return new GeoCoordinate(ToDegrees(num2), ToDegrees(radians));
        }

        public static GeoCoordinate CreateWaypoint(GeoCoordinate sourceLocation, double distanceInMeters,
            double bearingDegrees, double altitude)
        {
            var num1 = distanceInMeters/1000.0/6371.0;
            var rad1 = ToRad(bearingDegrees);
            var rad2 = ToRad(sourceLocation.Latitude);
            var rad3 = ToRad(sourceLocation.Longitude);
            var num2 = Math.Asin(Math.Sin(rad2)*Math.Cos(num1) + Math.Cos(rad2)*Math.Sin(num1)*Math.Cos(rad1));
            var num3 = Math.Atan2(Math.Sin(rad1)*Math.Sin(num1)*Math.Cos(rad2),
                Math.Cos(num1) - Math.Sin(rad2)*Math.Sin(num2));
            var radians = (rad3 + num3 + 3.0*Math.PI)%(2.0*Math.PI) - Math.PI;
            return new GeoCoordinate(ToDegrees(num2), ToDegrees(radians), altitude);
        }

        public static double DegreeBearing(GeoCoordinate sourceLocation, GeoCoordinate targetLocation)
        {
            var y = ToRad(targetLocation.Longitude - sourceLocation.Longitude);
            var x =
                Math.Log(Math.Tan(ToRad(targetLocation.Latitude)/2.0 + Math.PI/4.0)/
                         Math.Tan(ToRad(sourceLocation.Latitude)/2.0 + Math.PI/4.0));
            if (Math.Abs(y) > Math.PI)
            {
                y = y > 0.0 ? -(2.0*Math.PI - y) : 2.0*Math.PI + y;
            }
            return ToBearing(Math.Atan2(y, x));
        }

        public static double ToBearing(double radians)
        {
            return (ToDegrees(radians) + 360.0)%360.0;
        }

        public static double ToDegrees(double radians)
        {
            return radians*180.0/Math.PI;
        }

        public static double ToRad(double degrees)
        {
            return degrees*(Math.PI/180.0);
        }

        #endregion
    }
}