using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Barcode { get; set; }
        public int? SupplierId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; }
    }

}
