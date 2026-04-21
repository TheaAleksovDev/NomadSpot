using System.Threading.Tasks;

namespace NomadSpot.ViewModel
{
    public interface IGeolocationService
    {
        Task<(double Lat, double Lon)> GetLocationAsync();
    }
}
