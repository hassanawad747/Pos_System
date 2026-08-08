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
        public DataTable SalesTrends()=>Query(@"SELECT N'DAILY' period_type,CONVERT(NVARCHAR(10),CAST(sale_date AS date),120) period,currency,SUM(total_amount) revenue,COUNT_BIG(*) invoice_count FROM dbo.Sales WHERE ISNULL(sale_status,N'COMPLETED')<>N'VOID' GROUP BY CAST(sale_date AS date),currency
UNION ALL SELECT N'WEEKLY',CONCAT(DATEPART(year,sale_date),N'-W',RIGHT(N'0'+CONVERT(NVARCHAR(2),DATEPART(iso_week,sale_date)),2)),currency,SUM(total_amount),COUNT_BIG(*) FROM dbo.Sales WHERE ISNULL(sale_status,N'COMPLETED')<>N'VOID' GROUP BY DATEPART(year,sale_date),DATEPART(iso_week,sale_date),currency
UNION ALL SELECT N'MONTHLY',CONVERT(NVARCHAR(7),sale_date,120),currency,SUM(total_amount),COUNT_BIG(*) FROM dbo.Sales WHERE ISNULL(sale_status,N'COMPLETED')<>N'VOID' GROUP BY CONVERT(NVARCHAR(7),sale_date,120),currency ORDER BY period_type,period DESC;");
        public DataTable SlowDeadStock()=>Query(@"SELECT p.product_id,p.name product_name,ISNULL(SUM(ps.quantity),0) quantity,MAX(si.sale_date) last_sale_at,DATEDIFF(day,MAX(si.sale_date),SYSUTCDATETIME()) days_without_sale,CASE WHEN MAX(si.sale_date) IS NULL OR MAX(si.sale_date)<DATEADD(day,-90,SYSUTCDATETIME()) THEN N'DEAD' WHEN MAX(si.sale_date)<DATEADD(day,-30,SYSUTCDATETIME()) THEN N'SLOW' ELSE N'ACTIVE' END movement_status FROM dbo.Products p LEFT JOIN dbo.ProductStock ps ON ps.product_id=p.product_id LEFT JOIN dbo.Sale_Items si ON si.product_id=p.product_id GROUP BY p.product_id,p.name HAVING MAX(si.sale_date)<DATEADD(day,-30,SYSUTCDATETIME()) OR MAX(si.sale_date) IS NULL ORDER BY last_sale_at;");
        public DataTable Reorder()=>Query("SELECT * FROM dbo.vw_StockHealth WHERE quantity<=0 OR needs_reorder=1 ORDER BY quantity,product_name;");
        public DataTable Batches()=>Query("SELECT TOP(1000) * FROM dbo.vw_ExpiringBatches ORDER BY expiry_date,product_name;");
        public DataTable Losses()=>Query("SELECT TOP(1000) * FROM dbo.vw_InventoryLosses ORDER BY created_at DESC;");
        public DataTable CustomerProfitability()=>Query(@"SELECT s.customer_id,c.name,COUNT(DISTINCT s.sale_id) invoice_count,SUM(ISNULL(si.unit_price,0)*ISNULL(si.quantity_decimal,si.quantity)) revenue,SUM(ISNULL(si.unit_cost_snapshot,0)*ISNULL(si.base_quantity,si.quantity)) cogs,SUM(ISNULL(si.unit_price,0)*ISNULL(si.quantity_decimal,si.quantity)-ISNULL(si.unit_cost_snapshot,0)*ISNULL(si.base_quantity,si.quantity)) gross_profit FROM dbo.Sales s JOIN dbo.Customers c ON c.customer_id=s.customer_id JOIN dbo.Sale_Items si ON si.sale_id=s.sale_id WHERE ISNULL(s.sale_status,N'COMPLETED')<>N'VOID' GROUP BY s.customer_id,c.name ORDER BY gross_profit DESC;");
        public DataTable Cashiers()=>Query("SELECT TOP(1000) * FROM dbo.vw_CashierPerformance ORDER BY sale_date DESC,revenue DESC;");
        public DataTable Shifts()=>Query("SELECT TOP(1000) cash_session_id,register_id,user_id,opened_at,closed_at,expected_usd,actual_usd,difference_usd,expected_lbp,actual_lbp,difference_lbp,status FROM dbo.CashSessions ORDER BY opened_at DESC;");
        public DataTable ReturnAnalysis()=>Query("SELECT TOP(1000) * FROM dbo.vw_ReturnAnalysis ORDER BY return_date DESC;");
        public DataTable Discounts()=>Query("SELECT TOP(1000) * FROM dbo.vw_DiscountAnalysis ORDER BY sale_date DESC,discount_amount DESC;");
        public DataTable Returns()=>Query(@"SELECT TOP(500) r.return_number,r.sale_id,r.return_type,r.refund_method,r.currency,r.total_amount,r.reason,r.created_at,u.username
FROM dbo.SalesReturns r JOIN dbo.Users u ON u.user_id=r.user_id ORDER BY r.created_at DESC;");
        public DataTable Audit()=>Query(@"SELECT TOP(500) * FROM dbo.AuditLogs ORDER BY 1 DESC;");
        private DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
    }
}
