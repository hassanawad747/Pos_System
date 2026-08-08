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
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<SupplierTransaction> SupplierTransactions { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; }
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
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(x => x.User_Id);
            modelBuilder.Entity<User>().Property(x => x.User_Id).HasColumnName("user_id");
            modelBuilder.Entity<User>().Property(x => x.Username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(x => x.Password_Hash).HasColumnName("password_hash");
            modelBuilder.Entity<User>().Property(x => x.Role).HasColumnName("role");
            modelBuilder.Entity<User>().Property(x => x.Created_At).HasColumnName("created_at");
            modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();

            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Category>().HasKey(x => x.CategoryId);
            modelBuilder.Entity<Category>().Property(x => x.CategoryId).HasColumnName("category_id");
            modelBuilder.Entity<Category>().Property(x => x.CategoryName).HasColumnName("category_name");
            modelBuilder.Entity<Category>().Property(x => x.Description).HasColumnName("description");

            modelBuilder.Entity<Supplier>().ToTable("Suppliers");
            modelBuilder.Entity<Supplier>().HasKey(x => x.SupplierId);
            modelBuilder.Entity<Supplier>().Property(x => x.SupplierId).HasColumnName("supplier_id");
            modelBuilder.Entity<Supplier>().Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<Supplier>().Property(x => x.ContactInfo).HasColumnName("contact_info");
            modelBuilder.Entity<Supplier>().Property(x => x.Address).HasColumnName("address");

            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Customer>().HasKey(x => x.CustomerId);
            modelBuilder.Entity<Customer>().Property(x => x.CustomerId).HasColumnName("customer_id");
            modelBuilder.Entity<Customer>().Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<Customer>().Property(x => x.Phone).HasColumnName("phone");
            modelBuilder.Entity<Customer>().Property(x => x.Email).HasColumnName("email");
            modelBuilder.Entity<Customer>().Property(x => x.LoyaltyPoints).HasColumnName("loyalty_points");
            modelBuilder.Entity<Customer>().Property(x => x.CreatedAt).HasColumnName("created_at");

            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Product>().HasKey(x => x.ProductId);
            modelBuilder.Entity<Product>().Property(x => x.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<Product>().Property(x => x.Name).HasColumnName("name");
            modelBuilder.Entity<Product>().Property(x => x.CategoryId).HasColumnName("category_id");
            modelBuilder.Entity<Product>().Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(x => x.PurchasePrice).HasColumnName("purchase_price").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(x => x.SellingPrice).HasColumnName("selling_price").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(x => x.StockQuantity).HasColumnName("stock_quantity");
            modelBuilder.Entity<Product>().Property(x => x.Barcode).HasColumnName("barcode");
            modelBuilder.Entity<Product>().Property(x => x.SupplierId).HasColumnName("supplier_id");
            modelBuilder.Entity<Product>().Property(x => x.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Product>().Property(x => x.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<Product>().HasIndex(x => x.Barcode).IsUnique().HasFilter("[barcode] IS NOT NULL");
            modelBuilder.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Product>().HasOne(x => x.Supplier).WithMany(x => x.Products).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sale>().ToTable("Sales");
            modelBuilder.Entity<Sale>().HasKey(x => x.SaleId);
            modelBuilder.Entity<Sale>().Property(x => x.SaleId).HasColumnName("sale_id");
            modelBuilder.Entity<Sale>().Property(x => x.InvoiceNumber).HasColumnName("invoice_number");
            modelBuilder.Entity<Sale>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Sale>().Property(x => x.CustomerId).HasColumnName("customer_id");
            modelBuilder.Entity<Sale>().Property(x => x.SaleDate).HasColumnName("sale_date");
            modelBuilder.Entity<Sale>().Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Sale>().Property(x => x.PaymentMethod).HasColumnName("payment_method");
            modelBuilder.Entity<Sale>().Property(x => x.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Sale>().Property(x => x.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<Sale>().HasIndex(x => x.InvoiceNumber).IsUnique().HasFilter("[invoice_number] IS NOT NULL");
            modelBuilder.Entity<Sale>().HasOne(x => x.User).WithMany(x => x.Sales).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sale>().HasOne(x => x.Customer).WithMany(x => x.Sales).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SaleItem>().ToTable("SaleItems");
            modelBuilder.Entity<SaleItem>().HasKey(x => x.SaleItemId);
            modelBuilder.Entity<SaleItem>().Property(x => x.SaleItemId).HasColumnName("sale_item_id");
            modelBuilder.Entity<SaleItem>().Property(x => x.SaleId).HasColumnName("sale_id");
            modelBuilder.Entity<SaleItem>().Property(x => x.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<SaleItem>().Property(x => x.Quantity).HasColumnName("quantity");
            modelBuilder.Entity<SaleItem>().Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(x => x.Discount).HasColumnName("discount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(x => x.OriginalUnitPrice).HasColumnName("original_unit_price").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(x => x.DiscountType).HasColumnName("discount_type");
            modelBuilder.Entity<SaleItem>().Property(x => x.DiscountValue).HasColumnName("discount_value").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(x => x.DiscountBy).HasColumnName("discount_by");
            modelBuilder.Entity<SaleItem>().HasOne(x => x.Sale).WithMany(x => x.SaleItems).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SaleItem>().HasOne(x => x.Product).WithMany(x => x.SaleItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>().ToTable("Purchases");
            modelBuilder.Entity<Purchase>().HasKey(x => x.PurchaseId);
            modelBuilder.Entity<Purchase>().Property(x => x.PurchaseId).HasColumnName("purchase_id");
            modelBuilder.Entity<Purchase>().Property(x => x.InvoiceNumber).HasColumnName("invoice_number");
            modelBuilder.Entity<Purchase>().Property(x => x.SupplierId).HasColumnName("supplier_id");
            modelBuilder.Entity<Purchase>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Purchase>().Property(x => x.PurchaseDate).HasColumnName("purchase_date");
            modelBuilder.Entity<Purchase>().Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.PaidAmount).HasColumnName("paid_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.RemainingAmount).HasColumnName("remaining_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>().Property(x => x.PaymentStatus).HasColumnName("payment_status");
            modelBuilder.Entity<Purchase>().Property(x => x.PaymentMethod).HasColumnName("payment_method");
            modelBuilder.Entity<Purchase>().Property(x => x.Notes).HasColumnName("notes");
            modelBuilder.Entity<Purchase>().Property(x => x.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Purchase>().Property(x => x.UpdatedAt).HasColumnName("updated_at");
            modelBuilder.Entity<Purchase>().HasIndex(x => x.InvoiceNumber).IsUnique();
            modelBuilder.Entity<Purchase>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Purchase>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>().ToTable("PurchaseItems");
            modelBuilder.Entity<PurchaseItem>().HasKey(x => x.PurchaseItemId);
            modelBuilder.Entity<PurchaseItem>().Property(x => x.PurchaseItemId).HasColumnName("purchase_item_id");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.PurchaseId).HasColumnName("purchase_id");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.Quantity).HasColumnName("quantity");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PurchaseItem>().Property(x => x.LineTotal).HasColumnName("line_total").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PurchaseItem>().HasOne(x => x.Purchase).WithMany(x => x.PurchaseItems).HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PurchaseItem>().HasOne(x => x.Product).WithMany(x => x.PurchaseItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransaction>().ToTable("InventoryTransactions");
            modelBuilder.Entity<InventoryTransaction>().HasKey(x => x.InventoryTransactionId);
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.InventoryTransactionId).HasColumnName("inventory_transaction_id");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.TransactionType).HasColumnName("transaction_type");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.QuantityChange).HasColumnName("quantity_change");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.OldQuantity).HasColumnName("old_quantity");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.NewQuantity).HasColumnName("new_quantity");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.ReferenceType).HasColumnName("reference_type");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.ReferenceId).HasColumnName("reference_id");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.Notes).HasColumnName("notes");
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<InventoryTransaction>().HasOne(x => x.Product).WithMany(x => x.InventoryTransactions).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryTransaction>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierTransaction>().ToTable("SupplierTransactions");
            modelBuilder.Entity<SupplierTransaction>().HasKey(x => x.SupplierTransactionId);
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.SupplierTransactionId).HasColumnName("supplier_transaction_id");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.SupplierId).HasColumnName("supplier_id");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.TransactionType).HasColumnName("transaction_type");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.ReferenceType).HasColumnName("reference_type");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.ReferenceId).HasColumnName("reference_id");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.Debit).HasColumnName("debit").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.Credit).HasColumnName("credit").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.Currency).HasColumnName("currency");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.Description).HasColumnName("description");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<SupplierTransaction>().Property(x => x.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<SupplierTransaction>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierTransaction>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierPayment>().ToTable("SupplierPayments");
            modelBuilder.Entity<SupplierPayment>().HasKey(x => x.SupplierPaymentId);
            modelBuilder.Entity<SupplierPayment>().Property(x => x.SupplierPaymentId).HasColumnName("supplier_payment_id");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.SupplierId).HasColumnName("supplier_id");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.Currency).HasColumnName("currency");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.PaymentMethod).HasColumnName("payment_method");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.ReferenceNumber).HasColumnName("reference_number");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.Notes).HasColumnName("notes");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<SupplierPayment>().Property(x => x.PaymentDate).HasColumnName("payment_date");
            modelBuilder.Entity<SupplierPayment>().HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierPayment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Discount>().Property(x => x.Value).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Return>().Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InventoryLog>().HasKey(x => x.LogId);
            modelBuilder.Entity<BackupLog>().HasKey(x => x.BackupId);
            modelBuilder.Entity<SaleItemsBackup>().HasOne(x => x.SalesBackup).WithMany(x => x.SaleItemsBackups).HasForeignKey(x => x.SalesBackupId).OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
