using Microsoft.EntityFrameworkCore;
using VAM.Entities;

namespace VAM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SellerProfile> SellerProfiles { get; set; }
        public DbSet<BusinessProfile> BusinessProfiles { get; set; }
        public DbSet<PayoutTransaction> PayoutTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Global Query Filter for IsDeleted
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Farm>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<SellerProfile>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<BusinessProfile>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PayoutTransaction>().HasQueryFilter(e => !e.IsDeleted);
            
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<SellerProfile>()
                .HasOne(s => s.User)
                .WithOne(u => u.SellerProfile)
                .HasForeignKey<SellerProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SellerProfile>()
                .HasOne(s => s.VerifiedBy)
                .WithMany()
                .HasForeignKey(s => s.VerifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BusinessProfile>()
                .HasOne(b => b.User)
                .WithOne(u => u.BusinessProfile)
                .HasForeignKey<BusinessProfile>(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BusinessProfile>()
                .HasOne(b => b.VerifiedBy)
                .WithMany()
                .HasForeignKey(b => b.VerifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Farm)
                .WithMany()
                .HasForeignKey(p => p.FarmId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
