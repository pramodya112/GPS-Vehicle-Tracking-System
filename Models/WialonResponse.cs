using Newtonsoft.Json;
using System.Collections.Generic;

namespace CEBVehicleTracker.Models
{
    public class WialonResponse
    {
        [JsonProperty("items")]
        public List<Vehicle> Items { get; set; } = new List<Vehicle>();
    }

    public class Vehicle
{
    [JsonProperty("id")]
    public int Id { get; set; }

    // Wialon often uses "nm" for name
    [JsonProperty("nm")]
    public string Name { get; set; }

    [JsonProperty("pos")]
    public Position Position { get; set; }

    // Add this to track if we have valid data
    public bool HasValidPosition => Position != null && 
                                  Math.Abs(Position.Latitude) > 0.0001 && 
                                  Math.Abs(Position.Longitude) > 0.0001;

        public bool IsMoving { get; internal set; }
    }

    public class Position
    {
        [JsonProperty("y")]
        public double Latitude { get; set; }

        [JsonProperty("x")]
        public double Longitude { get; set; }

        [JsonProperty("s")]
        public double Speed { get; set; }
        public double Y { get; internal set; }
        public double X { get; internal set; }
    }

    public class WialonSession
    {
        [JsonProperty("eid")]
        public string Eid { get; set; } = string.Empty;
    }
}