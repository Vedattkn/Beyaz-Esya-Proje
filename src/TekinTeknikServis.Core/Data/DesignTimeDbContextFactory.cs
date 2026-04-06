using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TekinTeknikServis.Core.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var webPath = Path.Combine(basePath, "..", "TekinTeknikServis.Web");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.Exists(webPath) ? webPath : basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION")
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION_STRING")
                ?? config.GetConnectionString("SupabaseDb");

            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Password=CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Supabase DB connection string is missing. Set ConnectionStrings:SupabaseDb in TekinTeknikServis.Web/appsettings.json or SUPABASE_DB_CONNECTION environment variable.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}