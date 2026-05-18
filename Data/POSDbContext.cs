using Pos_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Pos_System.Data
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<BackupLog> BackupLogs { get; set; }
        public DbSet<SalesBackup> SalesBackups { get; set; }
        public DbSet<SaleItemsBackup> SaleItemsBackups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sales)
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(si => si.ProductId);

            modelBuilder.Entity<BackupLog>().HasNoKey();
            modelBuilder.Entity<InventoryLog>().HasNoKey();
            modelBuilder.Entity<SaleItemsBackup>()
                .HasOne(sib => sib.SalesBackup)
                .WithMany(sb => sb.SaleItemsBackups)
                .HasForeignKey(sib => sib.SalesBackupId);

            base.OnModelCreating(modelBuilder);


        }
    }

}
