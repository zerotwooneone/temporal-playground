using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TravelLogistics;

namespace TemporalDDD.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<CredentialEvaluation> CredentialEvaluations => Set<CredentialEvaluation>();
    public DbSet<ProviderProfile> ProviderProfiles => Set<ProviderProfile>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<FlightBooking> FlightBookings => Set<FlightBooking>();
    public DbSet<LodgingBooking> LodgingBookings => Set<LodgingBooking>();

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
