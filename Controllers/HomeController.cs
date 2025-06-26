using System.Collections.Generic;
using System.Threading.Tasks;
using CEBVehicleTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace CEBVehicleTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWialonService _wialonService;
        private readonly List<string> _targetVehicles;

        public HomeController(IWialonService wialonService, List<string> targetVehicles)
        {
            _wialonService = wialonService;
            _targetVehicles = targetVehicles;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var sid = await _wialonService.LoginAsync();
                var vehicles = await _wialonService.GetVehiclePositionsAsync(sid, _targetVehicles);
                
                ViewBag.Vehicles = _targetVehicles;
                return View(vehicles);
            }
            catch
            {
                // Handle error (show empty map or error message)
                ViewBag.Vehicles = _targetVehicles;
                return View(new List<Models.Vehicle>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicleData()
        {
            try
            {
                var sid = await _wialonService.LoginAsync();
                var vehicles = await _wialonService.GetVehiclePositionsAsync(sid, _targetVehicles);
                return Ok(vehicles);
            }
            catch
            {
                return BadRequest("Error fetching vehicle data");
            }
        }
    }
}