using System.Collections.Generic;
using System.Threading.Tasks;
using CEBVehicleTracker.Models;

namespace CEBVehicleTracker.Services
{
    public interface IWialonService
    {
        Task<string> LoginAsync();
        Task<List<Vehicle>> GetVehiclePositionsAsync(string sid, List<string> targetVehicles);
    }
}