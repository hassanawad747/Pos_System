using System;
using System.Collections.Generic;

namespace Pos_System.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }

        // Legacy selling-price column kept for backward compatibility.
        public decimal Price { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }

        public int StockQuantity { get; set; }
        public string Barcode { get; set; }
        public int? SupplierId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; }
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}
