using Microsoft.EntityFrameworkCore;

namespace TekinTeknikServis.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var categories = modelBuilder.Entity<CategoryEntity>();
            categories.ToTable("kategoriler", "public");
            categories.HasKey(x => x.Id);
            categories.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            categories.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            categories.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            categories.HasIndex(x => x.Name).IsUnique();
        }
    }
}