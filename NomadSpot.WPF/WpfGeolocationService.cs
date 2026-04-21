using NomadSpot.ViewModel;
using System.Threading.Tasks;

namespace NomadSpot.WPF
{
    public class WpfGeolocationService : IGeolocationService
    {
        public async Task<(double Lat, double Lon)> GetLocationAsync()
        {
            var geolocator = new Windows.Devices.Geolocation.Geolocator();
            var position = await geolocator.GetGeopositionAsync();
            return (position.Coordinate.Point.Position.Latitude,
                    position.Coordinate.Point.Position.Longitude);
        }
    }
}
