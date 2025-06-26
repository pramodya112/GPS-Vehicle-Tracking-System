using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CEBVehicleTracker.Models;

namespace CEBVehicleTracker.Services
{
    public class WialonService : IWialonService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = //  = "REMOVED_FOR_SECURITY"; //  removed intentionally
;

        public WialonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private string _sid;
        private DateTime _sidExpiry = DateTime.MinValue;

        public async Task<string> LoginAsync()
        {
            if (string.IsNullOrEmpty(_sid) || DateTime.Now > _sidExpiry)
            {
                var token = //  = "REMOVED_FOR_SECURITY"; // Token was removed intentionally

                var requestUrl = $"{BaseUrl}?svc=token/login&params={{\"token\":\"{token}\"}}";
                Console.WriteLine($"Login request URL: {requestUrl}");
                var response = await _httpClient.PostAsync(requestUrl, null);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Login failed: {response.StatusCode}");
                    throw new HttpRequestException($"Login failed: {response.StatusCode}");
                }
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login response: {content}");
                var session = JsonConvert.DeserializeObject<WialonSession>(content);
                _sid = session.Eid;
                _sidExpiry = DateTime.Now.AddMinutes(30);
                Console.WriteLine($"Login successful, SID: {_sid}, Expiry: {_sidExpiry}");
            }
            return _sid;
        }

        public async Task<List<Vehicle>> GetVehiclePositionsAsync(string sid, List<string> targetVehicles)
        {
            var vehicles = new List<Vehicle>();
            Console.WriteLine($"Target vehicles: {string.Join(", ", targetVehicles)}");

            // First get all units
            var searchUrl = $"{BaseUrl}?svc=core/search_items&params={{\"spec\":{{\"itemsType\":\"avl_unit\",\"propName\":\"sys_name\",\"propValueMask\":\"*\",\"sortType\":\"sys_name\"}},\"force\":1,\"flags\":1025,\"from\":0,\"to\":0}}&sid={sid}";
            Console.WriteLine($"Search request URL: {searchUrl}");
            var searchResponse = await _httpClient.PostAsync(searchUrl, null);
            if (!searchResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Search failed: {searchResponse.StatusCode}");
                throw new HttpRequestException($"Search failed: {searchResponse.StatusCode}");
            }

            var searchContent = await searchResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Search response: {searchContent}");
            var wialonResponse = JsonConvert.DeserializeObject<WialonResponse>(searchContent);

            if (wialonResponse?.Items != null)
            {
                Console.WriteLine($"Found {wialonResponse.Items.Count} vehicles in search response");
                foreach (var vehicle in wialonResponse.Items)
                {
                    Console.WriteLine($"Checking vehicle: ID={vehicle.Id}, Name={vehicle.Name}");
                    if (targetVehicles.Contains(vehicle.Name))
                    {
                        Console.WriteLine($"Matched target vehicle: {vehicle.Name}");
                        // Get detailed position for each vehicle
                        var positionUrl = $"{BaseUrl}?svc=core/search_item&params={{\"id\":{vehicle.Id},\"flags\":4194304}}&sid={sid}";
                        Console.WriteLine($"Position request URL for {vehicle.Name}: {positionUrl}");
                        var positionResponse = await _httpClient.PostAsync(positionUrl, null);
                        if (!positionResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Position request failed for {vehicle.Name}: {positionResponse.StatusCode}");
                            continue;
                        }

                        var positionContent = await positionResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Raw position content for {vehicle.Name}: {positionContent}");

                        var itemResponse = JsonConvert.DeserializeObject<WialonItemResponse>(positionContent);
                        Console.WriteLine($"Deserialized itemResponse for {vehicle.Name}: {JsonConvert.SerializeObject(itemResponse, Formatting.Indented)}");

                        if (itemResponse?.Item != null)
                        {
                            var detailedVehicle = new Vehicle
                            {
                                Id = itemResponse.Item.Id,
                                Name = itemResponse.Item.Name ?? vehicle.Name, // Fallback to search response name
                                Position = itemResponse.Item.Position,
                                IsMoving = itemResponse.Item.Position?.Speed > 0
                            };
                            Console.WriteLine($"Final Vehicle: ID={detailedVehicle.Id}, Name={detailedVehicle.Name}, HasValidPosition={detailedVehicle.HasValidPosition}");
                            if (detailedVehicle.HasValidPosition)
                            {
                                vehicles.Add(detailedVehicle);
                            }
                            else
                            {
                                Console.WriteLine($"Skipping vehicle {detailedVehicle.Name}: Invalid position");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No item data for {vehicle.Name} in position response");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Vehicle {vehicle.Name} not in targetVehicles");
                    }
                }
            }
            else
            {
                Console.WriteLine("No vehicles found in search response");
            }

            Console.WriteLine($"Returning {vehicles.Count} vehicles");
            return vehicles;
        }
    }

    public class WialonItemResponse
    {
        [JsonProperty("item")]
        public DetailedVehicle Item { get; set; }
    }

    public class DetailedVehicle
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nm")]
        public string Name { get; set; }

        [JsonProperty("pos")]
        public Position Position { get; set; }
    }
}