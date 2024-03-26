namespace Airlines.Models
{
    public class Flight
    {
        public int ID { get; set; }
        public string?TransportFlightCarrier { get; set; }
        public string? FlightNumber { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string? Price { get; set; }
    }
}
