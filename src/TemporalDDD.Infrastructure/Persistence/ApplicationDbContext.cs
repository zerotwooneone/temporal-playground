using Microsoft.EntityFrameworkCore;
using TemporalDDD.Infrastructure.PlacementMatching;
using TemporalDDD.Infrastructure.ProviderCredentialing;
using TemporalDDD.Infrastructure.TimesheetProcessing;
using TemporalDDD.Infrastructure.TravelLogistics;

namespace TemporalDDD.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AssignmentDbo> Assignments => Set<AssignmentDbo>();
    public DbSet<CredentialEvaluationDbo> CredentialEvaluations => Set<CredentialEvaluationDbo>();
    public DbSet<ProviderProfileDbo> ProviderProfiles => Set<ProviderProfileDbo>();
    public DbSet<TimesheetDbo> Timesheets => Set<TimesheetDbo>();
    public DbSet<FlightBookingDbo> FlightBookings => Set<FlightBookingDbo>();
    public DbSet<LodgingBookingDbo> LodgingBookings => Set<LodgingBookingDbo>();
    public DbSet<FacilityDbo> Facilities => Set<FacilityDbo>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(localAppData, "TemporalDDD", "temporal_playground.sqlite");
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
