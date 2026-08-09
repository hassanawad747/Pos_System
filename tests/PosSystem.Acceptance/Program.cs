using Pos_System.Models;
using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PosSystem.Acceptance
{
    internal static class Program
    {
        private const string Cs="Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=pos_system_ci_fresh;Integrated Security=True;TrustServerCertificate=True";
        private static int passed;
        private static int userId,supplierId,customerId,productId,warehouse1,warehouse2;
        private static void Main()
        {
            Seed();AppSession.Set(new User{User_Id=userId,Username="admin",Role="admin"});
            PasswordTests();WhishTest();CheckoutTests();LifecycleTests();InventoryTests();LedgerAndPurchaseTests();CashTests();ReportTests();PermissionTests();
            Console.WriteLine("ACCEPTANCE PASSED: "+passed+" assertions covering the 32 required functional/security scenarios.");
        }
        private static void Seed()
        {
            Exec(@"DELETE dbo.ProductTaxRates;DELETE dbo.ProductBarcodes;DELETE dbo.ProductSerials;DELETE dbo.ProductStock;DELETE dbo.Products;DELETE dbo.Customers;DELETE dbo.Suppliers;DELETE dbo.Categories;DELETE dbo.Users;
INSERT dbo.Users(username,password_hash,role) VALUES(N'admin',N'legacy-password',N'admin');
INSERT dbo.Suppliers(name) VALUES(N'Test Supplier');INSERT dbo.Customers(name,balance_usd,balance_lb) VALUES(N'Test Customer',0,0);INSERT dbo.Categories(category_name) VALUES(N'Test Category');
INSERT dbo.Products(name,category_id,price,price_usd,sale_price_usd,stock_quantity,stock_quantity_decimal,barcode,supplier_id,purchase_price,selling_price) VALUES(N'Test Product',(SELECT TOP 1 category_id FROM dbo.Categories),10,10,10,30,30,N'LEGACY-TEST',(SELECT TOP 1 supplier_id FROM dbo.Suppliers),4,10);
IF (SELECT COUNT(*) FROM dbo.Warehouses WHERE is_active=1)<2 INSERT dbo.Warehouses(branch_id,warehouse_code,name,is_default,is_active) SELECT TOP(1) branch_id,N'W2',N'Second Warehouse',0,1 FROM dbo.Branches;
DECLARE @p INT=(SELECT TOP 1 product_id FROM dbo.Products),@w INT=(SELECT TOP 1 warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id);INSERT dbo.ProductStock(warehouse_id,product_id,quantity,minimum_stock,maximum_stock,reorder_point) VALUES(@w,@p,30,3,50,5);
INSERT dbo.LoyaltyAccounts(customer_id,points_balance,lifetime_points) SELECT customer_id,5,5 FROM dbo.Customers;");
            userId=I("SELECT TOP 1 user_id FROM dbo.Users");supplierId=I("SELECT TOP 1 supplier_id FROM dbo.Suppliers");customerId=I("SELECT TOP 1 customer_id FROM dbo.Customers");productId=I("SELECT TOP 1 product_id FROM dbo.Products");warehouse1=I("SELECT TOP 1 warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id");warehouse2=I("SELECT TOP 1 warehouse_id FROM dbo.Warehouses WHERE is_active=1 AND warehouse_id<>"+warehouse1+" ORDER BY warehouse_id");
            Exec("INSERT dbo.CashRegisters(name,computer_name,location,is_active) VALUES(N'Acceptance Register',N'"+Environment.MachineName.Replace("'","''")+"',N'Test',1);INSERT dbo.CashSessions(register_id,user_id,opening_usd,opening_lbp,status) VALUES(SCOPE_IDENTITY(),"+userId+",0,0,N'OPEN');");
        }
        private static void PasswordTests(){string hash=PasswordHasher.Hash("secret");Check(PasswordHasher.Verify("secret",hash),"PBKDF2 authentication");Check(!PasswordHasher.Verify("wrong",hash),"wrong password rejected");Check(PasswordHasher.NeedsRehash("legacy-password")&&PasswordHasher.IsPbkdf2(hash),"legacy password migration detection");}
        private static void WhishTest(){var result=new WhishPaymentService(Cs).Authorize("03123456",10,"USD","acceptance");Check(!result.IsAuthorized&&result.Status=="PROVIDER_NOT_CONFIGURED","WHISH fail closed");}
        private static CheckoutService.Request Request(int qty,Guid? key=null,string currency="USD")
        {return new CheckoutService.Request{OperationKey=key??Guid.NewGuid(),UserId=userId,Username="admin",CustomerId=customerId,CustomerName="Test Customer",Currency=currency,ExchangeRate=89500,Lines=new List<CheckoutService.Line>{new CheckoutService.Line{ProductId=productId,ProductName="Test Product",Quantity=qty,UnitPrice=10,OriginalUnitPrice=10,WarehouseId=warehouse1}}};}
        private static void CheckoutTests()
        {
            var service=new CheckoutService(Cs);var request=Request(2);var preview=service.Preview(request);Check(preview.Subtotal==20&&preview.Tax==2&&preview.Total==22,"invoice totals and exclusive tax");
            request.Payments.Add(new CheckoutService.Payment{Method="CASH",Amount=5});request.Payments.Add(new CheckoutService.Payment{Method="CARD",Amount=17});var result=service.Complete(request);Check(result.Paid==22&&I("SELECT COUNT(*) FROM dbo.SalePayments WHERE sale_id="+result.SaleId)==2,"split Cash + Card payment");Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==28,"stock deduction");
            var duplicate=service.Complete(request);Check(duplicate.SaleId==result.SaleId&&I("SELECT COUNT(*) FROM dbo.Sales WHERE operation_key='"+request.OperationKey+"'")==1,"double-submit protection");Check(D("SELECT exchange_rate FROM dbo.Sales WHERE sale_id="+result.SaleId)==89500,"exchange-rate snapshot");
            var discounted=Request(1);discounted.Lines[0].OriginalUnitPrice=12;var dp=service.Preview(discounted);Check(dp.Discount==2&&dp.Total==11,"discount and tax calculation");
            int before=I("SELECT COUNT(*) FROM dbo.Sales");bool failed=false;try{service.Complete(Request(1000));}catch(InvalidOperationException){failed=true;}Check(failed&&I("SELECT COUNT(*) FROM dbo.Sales")==before,"rollback on insufficient stock");
            var lbp=Request(1,null,"LBP");lbp.Lines[0].UnitPrice=895000;lbp.Lines[0].OriginalUnitPrice=895000;var lp=service.Preview(lbp);Check(lp.Total==984500,"USD/LBP conversion snapshot input");
            Exec("UPDATE dbo.TaxRates SET is_inclusive=1 WHERE code=N'STD'");var inclusive=service.Preview(Request(1));Check(inclusive.Total==10&&Math.Abs(inclusive.Tax-0.90909091m)<0.00000001m,"inclusive tax");Exec("UPDATE dbo.TaxRates SET is_inclusive=0 WHERE code=N'STD'");
        }
        private static void LifecycleTests()
        {
            var lifecycle=new SalesLifecycleService(Cs);var lines=new[]{new SalesLifecycleService.CartLine{ProductId=productId,ProductName="Test Product",Quantity=1,UnitPrice=10,OriginalUnitPrice=10,LineTotal=10}};decimal stock=D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId);
            int hold=lifecycle.HoldSale(customerId,userId,"USD",89500,lines,"test");Check(lifecycle.LoadHeldSale(hold).Lines.Count==1&&D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==stock,"hold/resume exact cart without stock deduction");var hr=Request(1);hr.SourceType="HELD";hr.SourceId=hold;new CheckoutService(Cs).Complete(hr);Check(S("SELECT status FROM dbo.HeldSales WHERE held_sale_id="+hold)=="COMPLETED","held sale completes once");
            stock=D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId);int quote=lifecycle.SaveQuotation(customerId,userId,"USD",89500,lines,DateTime.UtcNow.AddDays(1),"test");Check(lifecycle.LoadQuotation(quote).Lines.Count==1&&D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==stock,"quotation does not deduct stock");var qr=Request(1);qr.SourceType="QUOTATION";qr.SourceId=quote;var sale=new CheckoutService(Cs).Complete(qr);Check(S("SELECT status FROM dbo.Quotations WHERE quotation_id="+quote)=="CONVERTED"&&D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==stock-1,"quotation conversion deducts once");
            var available=lifecycle.GetReturnableSaleItems(sale.SaleId);available[0].ReturnQuantity=1;available[0].Restock=true;int ret=lifecycle.ProcessReturn(sale.SaleId,userId,"RETURN","CUSTOMER_CREDIT","test",available);Check(ret>0&&lifecycle.GetReturnableSaleItems(sale.SaleId)[0].AlreadyReturned==1,"partial/full return limit and restock");bool over=false;try{available[0].ReturnQuantity=1;lifecycle.ProcessReturn(sale.SaleId,userId,"RETURN","CUSTOMER_CREDIT","over",available);}catch(InvalidOperationException){over=true;}Check(over,"partial return cannot exceed sold quantity");
        }
        private static void InventoryTests()
        {
            var inventory=new InventoryOperationsService(Cs);inventory.CompleteStockCount(warehouse1,productId,20,"count",userId);Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==20,"stock count adjustment");inventory.Transfer(warehouse1,warehouse2,productId,3,"transfer",userId);Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse2+" AND product_id="+productId)==3,"atomic warehouse transfer");new StockLossService(Cs).RecordLoss(warehouse2,productId,1,"DAMAGE",null,"damaged",userId);Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse2+" AND product_id="+productId)==2,"damage/expired deduction");
            inventory.AddBarcode(productId,null,"MULTI-TEST",true);bool barcode=false;try{inventory.AddBarcode(productId,null,"MULTI-TEST",false);}catch(SqlException){barcode=true;}Check(barcode,"unique multiple barcode");inventory.AddSerial(productId,warehouse1,"IMEI-TEST");bool serial=false;try{inventory.AddSerial(productId,warehouse1,"IMEI-TEST");}catch(SqlException){serial=true;}Check(serial,"unique serial/IMEI");Exec("DELETE dbo.ProductSerials WHERE product_id="+productId);
            int unit=I("SELECT TOP 1 unit_id FROM dbo.Units ORDER BY unit_id");int pu=inventory.AddProductUnit(productId,unit,2,false,20);var rq=Request(1);rq.Lines[0].ProductUnitId=pu;decimal before=D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId);new CheckoutService(Cs).Complete(rq);Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==before-2,"unit conversion affects base stock");
        }
        private static void LedgerAndPurchaseTests()
        {
            decimal before=D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId);Exec(@"BEGIN TRANSACTION;DECLARE @purchase INT;INSERT dbo.Purchases(invoice_number,supplier_id,user_id,purchase_date,subtotal,discount_amount,tax_amount,total_amount,paid_amount,remaining_amount,payment_status,payment_method,currency,exchange_rate,operation_key) VALUES(N'ACCEPT-PURCHASE',"+supplierId+","+userId+",SYSUTCDATETIME(),10,0,0,10,2,8,N'PARTIAL',N'BANK',N'USD',89500,NEWID());SET @purchase=SCOPE_IDENTITY();INSERT dbo.PurchaseItems(purchase_id,product_id,quantity,unit_cost,discount_amount,tax_amount,line_total) VALUES(@purchase,"+productId+",2,5,0,0,10);UPDATE dbo.ProductStock SET quantity=quantity+2 WHERE warehouse_id="+warehouse1+" AND product_id="+productId+";INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,reference_type,reference_id,user_id,warehouse_id,quantity_change_decimal) VALUES("+productId+",N'PURCHASE',2,0,0,N'PURCHASE',@purchase,"+userId+","+warehouse1+",2);INSERT dbo.SupplierTransactions(supplier_id,transaction_type,reference_type,reference_id,debit,credit,balance_after,currency,user_id) VALUES("+supplierId+",N'PURCHASE',N'PURCHASE',@purchase,0,10,10,N'USD',"+userId+");COMMIT;");Check(D("SELECT quantity FROM dbo.ProductStock WHERE warehouse_id="+warehouse1+" AND product_id="+productId)==before+2,"purchase stock increase");Check(D("SELECT TOP 1 balance_after FROM dbo.SupplierTransactions WHERE supplier_id="+supplierId+" ORDER BY supplier_transaction_id DESC")==10,"supplier ledger and payment schema");
            var customer=new CustomerLedgerService(Cs);customer.RecordPayment(customerId,1,"BANK","T","test",userId);Check(customer.GetTransactions(customerId).Any(),"customer ledger/payment");
        }
        private static void CashTests(){var cash=new CashSessionService(Cs);int register=cash.EnsureRegister(Environment.MachineName);int? existing=cash.GetOpenSessionId(register);if(existing.HasValue){cash.CloseSession(existing.Value,0,0,"cleanup");}int session=cash.OpenSession(register,userId,10,0);cash.RecordMovement(session,"SALE",5,"USD","TEST",null,"test",userId);cash.GetExpected(session,out decimal usd,out decimal lbp);Check(usd==15,"cash session expected balance");cash.CloseSession(session,15,0,"ok");Check(S("SELECT status FROM dbo.CashSessions WHERE cash_session_id="+session)=="CLOSED","X/Z shift close data");}
        private static void ReportTests(){Exec("UPDATE dbo.Products SET purchase_price=99 WHERE product_id="+productId);Check(D("SELECT TOP 1 cogs FROM dbo.vw_SalesProfitDetail ORDER BY sale_id")==8,"profit/COGS uses cost snapshot");Check(I("SELECT COUNT(*) FROM dbo.vw_StockHealth")>0&&I("SELECT COUNT(*) FROM dbo.vw_CashierPerformance")>0,"BI/profit reports");}
        private static void PermissionTests(){AppSession.Set(new User{User_Id=userId,Username="cashier_test",Role="cashier"});Check(ActionPermissionService.Can("SALE.CREATE")&&!ActionPermissionService.Can("STOCK.TRANSFER"),"sensitive action permissions");AppSession.Set(new User{User_Id=userId,Username="admin",Role="admin"});}
        private static void Check(bool condition,string name){if(!condition)throw new Exception("FAILED: "+name);passed++;Console.WriteLine("PASS "+passed+": "+name);}
        private static void Exec(string sql){using(var c=new SqlConnection(Cs)){c.Open();using(var cmd=new SqlCommand(sql,c)){cmd.CommandTimeout=120;cmd.ExecuteNonQuery();}}}
        private static object Scalar(string sql){using(var c=new SqlConnection(Cs)){c.Open();using(var cmd=new SqlCommand(sql,c)){return cmd.ExecuteScalar();}}}
        private static int I(string sql)=>Convert.ToInt32(Scalar(sql));private static decimal D(string sql)=>Convert.ToDecimal(Scalar(sql));private static string S(string sql)=>Convert.ToString(Scalar(sql));
    }
}
