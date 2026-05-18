using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal? OriginalUnitPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public string DiscountBy { get; set; }

        // Navigation
        public Sale Sale { get; set; }
        public Product Product { get; set; }
    }

}
