using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal sealed class BusinessIntelligenceService
    {
        private readonly string cs;
        public BusinessIntelligenceService(string connectionString){cs=connectionString;}
        public DataTable DailyProfit()=>Query("SELECT TOP(366) report_date,currency,revenue,cogs,gross_profit,expenses,net_profit FROM dbo.vw_DailyProfitSummary ORDER BY report_date DESC,currency;");
        public DataTable ProductPerformance()=>Query("SELECT TOP(500) product_id,product_name,quantity_sold,gross_sales,discount_amount,last_sale_at FROM dbo.vw_ProductSalesPerformance ORDER BY gross_sales DESC;");
        public DataTable StockHealth()=>Query("SELECT TOP(1000) product_id,product_name,warehouse_name,quantity,minimum_stock,maximum_stock,reorder_point,needs_reorder,stock_value_cost,stock_value_retail FROM dbo.vw_StockHealth ORDER BY needs_reorder DESC,stock_value_cost DESC;");
        public DataTable Customers()=>Query("SELECT TOP(500) customer_id,name,invoice_count,lifetime_sales,last_sale_at,balance_usd,balance_lbp,loyalty_points FROM dbo.vw_CustomerBusinessSummary ORDER BY lifetime_sales DESC;");
        public DataTable Suppliers()=>Query("SELECT TOP(500) supplier_id,name,purchase_count,lifetime_purchases,last_purchase_at,ledger_balance FROM dbo.vw_SupplierBusinessSummary ORDER BY lifetime_purchases DESC;");
        public DataTable Hourly()=>Query("SELECT sale_hour,invoice_count,revenue FROM dbo.vw_HourlySales ORDER BY sale_hour;");
        public DataTable Returns()=>Query(@"SELECT TOP(500) r.return_number,r.sale_id,r.return_type,r.refund_method,r.currency,r.total_amount,r.reason,r.created_at,u.username
FROM dbo.SalesReturns r JOIN dbo.Users u ON u.user_id=r.user_id ORDER BY r.created_at DESC;");
        public DataTable Audit()=>Query(@"SELECT TOP(500) * FROM dbo.AuditLogs ORDER BY 1 DESC;");
        private DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
    }
}
