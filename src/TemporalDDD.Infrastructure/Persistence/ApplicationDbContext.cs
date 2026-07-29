using Microsoft.EntityFrameworkCore;

namespace TemporalDDD.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(localAppData, "TemporalDDD", "temporal_playground.sqlite");
            
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
