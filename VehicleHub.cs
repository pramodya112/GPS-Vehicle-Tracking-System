using Microsoft.AspNetCore.SignalR;
using CEBVehicleTracker.Services;
using System.Threading.Tasks;

public class VehicleHub : Hub
{
    private readonly IWialonService _wialonService;
    private readonly List<string> _targetVehicles;

    public VehicleHub(IWialonService wialonService, List<string> targetVehicles)
    {
        _wialonService = wialonService;
        _targetVehicles = targetVehicles;
    }

    public async Task StartTracking()
    {
        var sid = await _wialonService.LoginAsync();
        while (true)
        {
            var vehicles = await _wialonService.GetVehiclePositionsAsync(sid, _targetVehicles);
            await Clients.All.SendAsync("ReceiveVehicleUpdate", vehicles);
            await Task.Delay(2000); // Poll every 2 seconds
        }
    }
}