using System;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal sealed class PricingService
    {
        private readonly string cs;
        public PricingService(string connectionString){cs=connectionString;}
        public DataTable TaxRates()=>Query("SELECT tax_rate_id,code,name,rate_percent,is_inclusive,is_active,effective_from,effective_to FROM dbo.TaxRates ORDER BY name;");
        public DataTable ExchangeRates()=>Query("SELECT TOP(200) exchange_rate_id,from_currency,to_currency,rate,effective_from,effective_to,created_at FROM dbo.ExchangeRates ORDER BY effective_from DESC;");
        public DataTable PriceLevels()=>Query("SELECT price_level_id,code,name,discount_percent,is_active FROM dbo.CustomerPriceLevels ORDER BY name;");
        public DataTable Promotions()=>Query("SELECT promotion_id,code,name,promotion_type,priority,start_at,end_at,is_active,stackable FROM dbo.Promotions ORDER BY priority,start_at DESC;");
        public DataTable Loyalty()=>Query("SELECT la.loyalty_account_id,c.customer_id,c.name,la.points_balance,la.lifetime_points,la.tier,la.updated_at FROM dbo.LoyaltyAccounts la JOIN dbo.Customers c ON c.customer_id=la.customer_id ORDER BY c.name;");
        public DataTable Customers()=>Query("SELECT customer_id,name FROM dbo.Customers ORDER BY name;");
        public DataTable Products()=>Query("SELECT product_id,name FROM dbo.Products ORDER BY name;");

        public int AddExchangeRate(string from,string to,decimal rate,DateTime effective,int? userId)
        {
            if(rate<=0)throw new InvalidOperationException("Exchange rate must be greater than zero.");
            from=(from??"").Trim().ToUpperInvariant();to=(to??"").Trim().ToUpperInvariant();
            using(var c=Open())using(var tx=c.BeginTransaction())
            {try{
                using(var close=new SqlCommand("UPDATE dbo.ExchangeRates SET effective_to=@e WHERE from_currency=@f AND to_currency=@t AND effective_to IS NULL AND effective_from<@e;",c,tx)){close.Parameters.AddWithValue("@e",effective);close.Parameters.AddWithValue("@f",from);close.Parameters.AddWithValue("@t",to);close.ExecuteNonQuery();}
                using(var cmd=new SqlCommand("INSERT dbo.ExchangeRates(from_currency,to_currency,rate,effective_from,user_id) VALUES(@f,@t,@r,@e,@u);SELECT CAST(SCOPE_IDENTITY() AS INT);",c,tx)){cmd.Parameters.AddWithValue("@f",from);cmd.Parameters.AddWithValue("@t",to);Dec(cmd,"@r",rate);cmd.Parameters.AddWithValue("@e",effective);cmd.Parameters.AddWithValue("@u",userId.HasValue?(object)userId.Value:DBNull.Value);int id=Convert.ToInt32(cmd.ExecuteScalar());tx.Commit();return id;}
            }catch{tx.Rollback();throw;}}
        }

        public int AddTaxRate(string code,string name,decimal percent,bool inclusive)
        {
            if(string.IsNullOrWhiteSpace(code)||string.IsNullOrWhiteSpace(name))throw new InvalidOperationException("Tax code and name are required."); if(percent<0||percent>100)throw new InvalidOperationException("Tax percent must be between 0 and 100.");
            using(var c=Open())using(var cmd=new SqlCommand("INSERT dbo.TaxRates(code,name,rate_percent,is_inclusive,is_active,effective_from) VALUES(@code,@name,@rate,@inc,1,SYSUTCDATETIME());SELECT CAST(SCOPE_IDENTITY() AS INT);",c)){cmd.Parameters.AddWithValue("@code",code.Trim().ToUpperInvariant());cmd.Parameters.AddWithValue("@name",name.Trim());Dec(cmd,"@rate",percent,9,4);cmd.Parameters.AddWithValue("@inc",inclusive);return Convert.ToInt32(cmd.ExecuteScalar());}
        }

        public int AddPriceLevel(string code,string name,decimal discount)
        {
            using(var c=Open())using(var cmd=new SqlCommand("INSERT dbo.CustomerPriceLevels(code,name,discount_percent,is_active) VALUES(@c,@n,@d,1);SELECT CAST(SCOPE_IDENTITY() AS INT);",c)){cmd.Parameters.AddWithValue("@c",code.Trim().ToUpperInvariant());cmd.Parameters.AddWithValue("@n",name.Trim());Dec(cmd,"@d",discount,9,4);return Convert.ToInt32(cmd.ExecuteScalar());}
        }

        public int AddPromotion(string code,string name,string type,decimal? discountPercent,decimal? minInvoice,DateTime? start,DateTime? end)
        {
            using(var c=Open())using(var tx=c.BeginTransaction())
            {try{int id;using(var cmd=new SqlCommand("INSERT dbo.Promotions(code,name,promotion_type,start_at,end_at,is_active) VALUES(@c,@n,@t,@s,@e,1);SELECT CAST(SCOPE_IDENTITY() AS INT);",c,tx)){cmd.Parameters.AddWithValue("@c",code.Trim().ToUpperInvariant());cmd.Parameters.AddWithValue("@n",name.Trim());cmd.Parameters.AddWithValue("@t",type.Trim().ToUpperInvariant());cmd.Parameters.AddWithValue("@s",start.HasValue?(object)start.Value:DBNull.Value);cmd.Parameters.AddWithValue("@e",end.HasValue?(object)end.Value:DBNull.Value);id=Convert.ToInt32(cmd.ExecuteScalar());}
                using(var cmd=new SqlCommand("INSERT dbo.PromotionRules(promotion_id,min_invoice_amount,discount_percent) VALUES(@p,@min,@disc);",c,tx)){cmd.Parameters.AddWithValue("@p",id);NullableDec(cmd,"@min",minInvoice);NullableDec(cmd,"@disc",discountPercent,9,4);cmd.ExecuteNonQuery();}tx.Commit();return id;
            }catch{tx.Rollback();throw;}}
        }

        public void AdjustLoyalty(int customerId,decimal points,string description,int userId)
        {
            using(var c=Open())using(var tx=c.BeginTransaction(IsolationLevel.Serializable))
            {try{int accountId;decimal before;
                using(var cmd=new SqlCommand("SELECT loyalty_account_id,points_balance FROM dbo.LoyaltyAccounts WITH(UPDLOCK,HOLDLOCK) WHERE customer_id=@c;",c,tx)){cmd.Parameters.AddWithValue("@c",customerId);using(var r=cmd.ExecuteReader()){if(r.Read()){accountId=Convert.ToInt32(r[0]);before=Convert.ToDecimal(r[1]);}else{r.Close();using(var create=new SqlCommand("INSERT dbo.LoyaltyAccounts(customer_id,points_balance,lifetime_points) VALUES(@c,0,0);SELECT CAST(SCOPE_IDENTITY() AS INT);",c,tx)){create.Parameters.AddWithValue("@c",customerId);accountId=Convert.ToInt32(create.ExecuteScalar());before=0;}}}}
                decimal after=before+points;if(after<0)throw new InvalidOperationException("Loyalty balance cannot become negative.");
                using(var cmd=new SqlCommand("UPDATE dbo.LoyaltyAccounts SET points_balance=@b,lifetime_points=lifetime_points+CASE WHEN @points>0 THEN @points ELSE 0 END,updated_at=SYSUTCDATETIME() WHERE loyalty_account_id=@id;",c,tx)){Dec(cmd,"@b",after);Dec(cmd,"@points",points);cmd.Parameters.AddWithValue("@id",accountId);cmd.ExecuteNonQuery();}
                using(var cmd=new SqlCommand("INSERT dbo.LoyaltyTransactions(loyalty_account_id,transaction_type,points,balance_after,reference_type,description,user_id) VALUES(@a,N'ADJUSTMENT',@p,@b,N'MANUAL',@d,@u);",c,tx)){cmd.Parameters.AddWithValue("@a",accountId);Dec(cmd,"@p",points);Dec(cmd,"@b",after);cmd.Parameters.AddWithValue("@d",string.IsNullOrWhiteSpace(description)?(object)DBNull.Value:description);cmd.Parameters.AddWithValue("@u",userId);cmd.ExecuteNonQuery();}tx.Commit();
            }catch{tx.Rollback();throw;}}
        }

        public void SetProductPrice(int productId,int priceLevelId,string currency,decimal price)
        {
            if(price<0)throw new InvalidOperationException("Price cannot be negative.");using(var c=Open())using(var cmd=new SqlCommand("INSERT dbo.ProductPrices(product_id,price_level_id,currency,price,effective_from) VALUES(@p,@l,@c,@price,SYSUTCDATETIME());",c)){cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@l",priceLevelId);cmd.Parameters.AddWithValue("@c",currency);Dec(cmd,"@price",price);cmd.ExecuteNonQuery();}
        }

        private SqlConnection Open(){var c=new SqlConnection(cs);c.Open();return c;}
        private DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
        private static void Dec(SqlCommand cmd,string n,decimal v,byte precision=24,byte scale=8){var p=cmd.Parameters.Add(n,SqlDbType.Decimal);p.Precision=precision;p.Scale=scale;p.Value=v;}
        private static void NullableDec(SqlCommand cmd,string n,decimal? v,byte precision=24,byte scale=8){var p=cmd.Parameters.Add(n,SqlDbType.Decimal);p.Precision=precision;p.Scale=scale;p.Value=v.HasValue?(object)v.Value:DBNull.Value;}
    }
}
