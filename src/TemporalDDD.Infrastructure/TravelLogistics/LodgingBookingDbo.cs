namespace TemporalDDD.Infrastructure.TravelLogistics;

public class LodgingBookingDbo
{
    public string Id { get; set; }
    public string? PublicId { get; set; }
    public string HotelName { get; set; }
    public string AddressStreet { get; set; }
    public string AddressCity { get; set; }
    public string AddressState { get; set; }
    public string AddressZipCode { get; set; }
    public long StayPeriodStartUtc { get; set; } // Unix milliseconds
    public long StayPeriodEndUtc { get; set; } // Unix milliseconds
    public string CostAmount { get; set; }
    public string CostCurrency { get; set; }
    public int Status { get; set; }
    public long BookedAt { get; set; } // Unix milliseconds
}
