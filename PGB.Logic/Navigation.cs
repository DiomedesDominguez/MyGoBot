namespace PGB.Logic
{
    using System;
    using System.Device.Location;
    using System.Threading;
    using System.Threading.Tasks;

    using PokemonGo.RocketAPI;

    using POGOProtos.Networking.Responses;

    using Utils;

    public class Navigation
    {
        #region Constructors

        public Navigation(Client client)
        {
            _client = client;
        }

        #endregion

        #region Fields, properties, indexers and constants

        private const double SpeedDownTo = 2.77777777777778;
        private readonly Client _client;

        #endregion

        #region Methods and other members

        public async Task<PlayerUpdateResponse> Move(GeoCoordinate targetLocation,
            double walkingSpeedInKilometersPerHour, Func<Task> functionExecutedWhileWalking,
            CancellationToken cancellationToken, bool disableHumanLikeWalking)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (disableHumanLikeWalking)
            {
                return
                    await
                        _client.Player.UpdatePlayerLocation(targetLocation.Latitude, targetLocation.Longitude,
                            _client.Settings.DefaultAltitude);
            }

            var speedInMetersPerSecond = walkingSpeedInKilometersPerHour/3.6;
            var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
            var waypoint1 = LocationUtils.CreateWaypoint(sourceLocation, speedInMetersPerSecond,
                LocationUtils.DegreeBearing(sourceLocation, targetLocation));
            var requestSendDateTime = DateTime.Now;
            var result =
                await
                    _client.Player.UpdatePlayerLocation(waypoint1.Latitude, waypoint1.Longitude,
                        _client.Settings.DefaultAltitude);
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                var totalMilliseconds = (DateTime.Now - requestSendDateTime).TotalMilliseconds;
                sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                var distanceInMeters = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
                var num = 40.0;
                if (distanceInMeters < num && speedInMetersPerSecond > 25.0/9.0)
                {
                    speedInMetersPerSecond = 25.0/9.0;
                }
                var val2 = totalMilliseconds/1000.0*speedInMetersPerSecond;
                var waypoint2 = LocationUtils.CreateWaypoint(sourceLocation, Math.Min(distanceInMeters, val2),
                    LocationUtils.DegreeBearing(sourceLocation, targetLocation));
                requestSendDateTime = DateTime.Now;
                result =
                    await
                        _client.Player.UpdatePlayerLocation(waypoint2.Latitude, waypoint2.Longitude,
                            _client.Settings.DefaultAltitude);
                if (functionExecutedWhileWalking != null)
                {
                    await functionExecutedWhileWalking();
                }
            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30.0);

            return result;
        }

        #endregion
    }
}