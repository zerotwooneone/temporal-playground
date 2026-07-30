namespace TemporalDDD.Infrastructure.TravelLogistics;

public class FlightBookingDbo
{
    public uint Id { get; set; }
    public string? PublicId { get; set; }
    public string FlightNumber { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public long DepartureTime { get; set; } // Unix milliseconds
    public string CostAmount { get; set; }
    public string CostCurrency { get; set; }
    public int Status { get; set; }
    public long BookedAt { get; set; } // Unix milliseconds
}
