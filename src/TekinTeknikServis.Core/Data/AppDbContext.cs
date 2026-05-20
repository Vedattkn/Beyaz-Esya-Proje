using Microsoft.EntityFrameworkCore;

namespace TekinTeknikServis.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
        public DbSet<OrderEntity> Orders => Set<OrderEntity>();
        public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
        public DbSet<ServiceRequestEntity> ServiceRequests => Set<ServiceRequestEntity>();

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

            var orders = modelBuilder.Entity<OrderEntity>();
            orders.ToTable("siparisler", "public");
            orders.HasKey(x => x.Id);
            orders.Property(x => x.Id)
                .HasColumnName("id");
            orders.Property(x => x.OrderNo)
                .HasColumnName("order_no")
                .HasMaxLength(40)
                .IsRequired();
            orders.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(320)
                .IsRequired();
            orders.Property(x => x.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(200)
                .IsRequired();
            orders.Property(x => x.TotalTry)
                .HasColumnName("total_try")
                .IsRequired();
            orders.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            var orderItems = modelBuilder.Entity<OrderItemEntity>();
            orderItems.ToTable("siparis_kalemleri", "public");
            orderItems.HasKey(x => x.Id);
            orderItems.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            orderItems.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();
            orderItems.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasMaxLength(100)
                .IsRequired();
            orderItems.Property(x => x.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(200)
                .IsRequired();
            orderItems.Property(x => x.PriceText)
                .HasColumnName("price_text")
                .HasMaxLength(50)
                .IsRequired();
            orderItems.Property(x => x.UnitPriceTry)
                .HasColumnName("unit_price_try")
                .IsRequired();
            orderItems.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();
            orderItems.Property(x => x.LineTotalTry)
                .HasColumnName("line_total_try")
                .IsRequired();

            orderItems.HasIndex(x => x.OrderId);
            orderItems.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            var serviceRequests = modelBuilder.Entity<ServiceRequestEntity>();
            serviceRequests.ToTable("servis_talepleri", "public");
            serviceRequests.HasKey(x => x.Id);
            serviceRequests.Property(x => x.Id).HasColumnName("id");
            serviceRequests.Property(x => x.KullaniciId).HasColumnName("kullanici_id");
            serviceRequests.Property(x => x.AdSoyad).HasColumnName("ad_soyad").HasMaxLength(200);
            serviceRequests.Property(x => x.Telefon).HasColumnName("telefon").HasMaxLength(20);
            serviceRequests.Property(x => x.CihazTuru).HasColumnName("cihaz_turu").HasMaxLength(120);
            serviceRequests.Property(x => x.ArizaAciklamasi).HasColumnName("ariza_aciklamasi");
            serviceRequests.Property(x => x.Durum).HasColumnName("durum").HasMaxLength(80);
            serviceRequests.Property(x => x.AdminCevabi).HasColumnName("admin_cevabi");
            serviceRequests.Property(x => x.KullaniciCevabi).HasColumnName("kullanici_cevabi");
            serviceRequests.Property(x => x.KayitTarihi).HasColumnName("kayit_tarihi");
            serviceRequests.Property(x => x.CustomerEmail).HasColumnName("customer_email").HasMaxLength(320);
            serviceRequests.Property(x => x.FaultyPart).HasColumnName("faulty_part").HasMaxLength(200);
            serviceRequests.Property(x => x.ReplacementPart).HasColumnName("replacement_part").HasMaxLength(200);
            serviceRequests.Property(x => x.RepairDetails).HasColumnName("repair_details");
            serviceRequests.Property(x => x.LaborPriceTry).HasColumnName("labor_price").HasColumnType("numeric(12,2)");
            serviceRequests.Property(x => x.PartPriceTry).HasColumnName("part_price").HasColumnType("numeric(12,2)");
            serviceRequests.Property(x => x.TotalPriceTry).HasColumnName("total_price").HasColumnType("numeric(12,2)");
            serviceRequests.Property(x => x.AdminNotes).HasColumnName("admin_notes");
            serviceRequests.Property(x => x.ApprovalStatus).HasColumnName("approval_status").HasMaxLength(60);
            serviceRequests.Property(x => x.ApprovalDate).HasColumnName("approval_date");
            serviceRequests.Property(x => x.ApprovalToken).HasColumnName("approval_token").HasMaxLength(120);
            serviceRequests.Property(x => x.ApprovalRequestedAt).HasColumnName("approval_requested_at");

        }
    }
}