param([string]$SqlInstance='(localdb)\MSSQLLocalDB')
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$fresh='pos_system_ci_fresh';$upgrade='pos_system_ci_upgrade'
function Open([string]$db){$c=New-Object Data.SqlClient.SqlConnection "Data Source=$SqlInstance;Initial Catalog=$db;Integrated Security=True;TrustServerCertificate=True";$c.Open();return $c}
function NonQuery([string]$sql,[string]$db='master'){$c=Open $db;try{$cmd=$c.CreateCommand();$cmd.CommandTimeout=300;$cmd.CommandText=$sql;[void]$cmd.ExecuteNonQuery()}finally{$c.Dispose()}}
function Scalar([string]$sql,[string]$db){$c=Open $db;try{$cmd=$c.CreateCommand();$cmd.CommandTimeout=300;$cmd.CommandText=$sql;return $cmd.ExecuteScalar()}finally{$c.Dispose()}}
function File([string]$path,[string]$db){$text=Get-Content $path -Raw -Encoding UTF8;$batches=[regex]::Split($text,'(?im)^\s*GO\s*(?:--.*)?$');$c=Open $db;try{foreach($batch in $batches){if([string]::IsNullOrWhiteSpace($batch)){continue};$cmd=$c.CreateCommand();$cmd.CommandTimeout=300;$cmd.CommandText=$batch;[void]$cmd.ExecuteNonQuery()}}finally{$c.Dispose()}}
function Recreate([string]$db){$safe=$db.Replace(']',']]');NonQuery "IF DB_ID(N'$db') IS NOT NULL BEGIN ALTER DATABASE [$safe] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;DROP DATABASE [$safe];END;CREATE DATABASE [$safe];"}
function Migrations(){return @(Get-ChildItem (Join-Path $repo 'database\migrations') -Filter '*.sql' -File|Sort-Object Name)}
Recreate $fresh;File (Join-Path $repo 'database\install\000_base_schema.sql') $fresh
foreach($migration in (Migrations)){Write-Host "FRESH -> $($migration.Name)";File $migration.FullName $fresh}
Recreate $upgrade;File (Join-Path $repo 'database\install\000_base_schema.sql') $upgrade
$all=Migrations
$latest=$all[-1]
foreach($migration in @($all|Select-Object -SkipLast 1)){Write-Host "UPGRADE BASE -> $($migration.Name)";File $migration.FullName $upgrade}
NonQuery "INSERT dbo.Settings(key_name,value) VALUES(N'ci_preserve_sentinel',N'KEEP_ME');" $upgrade
Write-Host "UPGRADE LATEST -> $($latest.Name)";File $latest.FullName $upgrade
if([string](Scalar "SELECT value FROM dbo.Settings WHERE key_name=N'ci_preserve_sentinel';" $upgrade) -ne 'KEEP_ME'){throw 'Existing-data upgrade did not preserve the sentinel row.'}
$count=$all.Count
foreach($db in @($fresh,$upgrade)){if([int](Scalar 'SELECT COUNT(*) FROM dbo.SchemaMigrations;' $db) -ne $count){throw "$db migration count does not match repository count $count."}}
foreach($db in @($fresh,$upgrade)){if([int](Scalar "SELECT COUNT(*) FROM sys.columns WHERE object_id IN (OBJECT_ID(N'dbo.Customers'),OBJECT_ID(N'dbo.Users')) AND name=N'created_by';" $db) -ne 2){throw "$db is missing a creator audit column required by a maintained form."}}
$behavior=@"
BEGIN TRANSACTION;
DECLARE @u INT,@supplier INT,@customer INT,@category INT,@product INT,@warehouse INT,@sale INT,@quotation INT;
INSERT dbo.Users(username,password_hash,role) VALUES(N'accept_admin',N'PBKDF2`$1`$100000`$salt`$hash',N'admin');SET @u=SCOPE_IDENTITY();
INSERT dbo.Suppliers(name) VALUES(N'Acceptance Supplier');SET @supplier=SCOPE_IDENTITY();
INSERT dbo.Customers(name,balance_usd,balance_lb) VALUES(N'Acceptance Customer',0,0);SET @customer=SCOPE_IDENTITY();
INSERT dbo.Categories(category_name) VALUES(N'Acceptance Category');SET @category=SCOPE_IDENTITY();
INSERT dbo.Products(name,category_id,price,price_usd,stock_quantity,barcode,supplier_id,purchase_price,selling_price,stock_quantity_decimal) VALUES(N'Acceptance Product',@category,15,15,10,N'LEGACY-A',@supplier,6,15,10);SET @product=SCOPE_IDENTITY();
SELECT TOP(1) @warehouse=warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id;
INSERT dbo.ProductStock(warehouse_id,product_id,quantity,minimum_stock,maximum_stock,reorder_point) VALUES(@warehouse,@product,10,3,20,4);
INSERT dbo.ProductBarcodes(product_id,barcode,is_primary) VALUES(@product,N'MULTI-A',1);
BEGIN TRY INSERT dbo.ProductBarcodes(product_id,barcode,is_primary) VALUES(@product,N'MULTI-A',0);THROW 53001,'Duplicate barcode was accepted.',1;END TRY BEGIN CATCH IF ERROR_NUMBER()=53001 THROW;END CATCH;
INSERT dbo.ProductSerials(product_id,warehouse_id,serial_number,status) VALUES(@product,@warehouse,N'IMEI-A',N'IN_STOCK');
BEGIN TRY INSERT dbo.ProductSerials(product_id,warehouse_id,serial_number,status) VALUES(@product,@warehouse,N'IMEI-A',N'IN_STOCK');THROW 53002,'Duplicate serial was accepted.',1;END TRY BEGIN CATCH IF ERROR_NUMBER()=53002 THROW;END CATCH;
INSERT dbo.Quotations(quotation_number,customer_id,user_id,currency,exchange_rate,subtotal,discount_total,tax_total,total_amount,status) VALUES(N'Q-ACCEPT',@customer,@u,N'USD',89500,15,0,0,15,N'OPEN');SET @quotation=SCOPE_IDENTITY();
INSERT dbo.QuotationItems(quotation_id,product_id,quantity,unit_price,original_unit_price,discount_amount,tax_amount,line_total) VALUES(@quotation,@product,1,15,15,0,0,15);
IF (SELECT quantity FROM dbo.ProductStock WHERE warehouse_id=@warehouse AND product_id=@product)<>10 THROW 53003,'Quotation incorrectly changed stock.',1;
INSERT dbo.Sales(user_id,customer_id,total_amount,payment_method,currency,exchange_rate,subtotal,discount_total,tax_total,paid_amount,remaining_amount,sale_status,operation_key) VALUES(@u,@customer,15,N'CASH',N'USD',89500,15,0,0,15,0,N'COMPLETED',NEWID());SET @sale=SCOPE_IDENTITY();
INSERT dbo.Sale_Items(sale_id,product_id,quantity,unit_price,unit_cost_snapshot,name_product,quantity_decimal,base_quantity,warehouse_id,tax_amount) VALUES(@sale,@product,1,15,6,N'Acceptance Product',1,1,@warehouse,0);
UPDATE dbo.Products SET purchase_price=99 WHERE product_id=@product;
IF NOT EXISTS(SELECT 1 FROM dbo.vw_SalesProfitDetail WHERE sale_id=@sale AND cogs=6 AND gross_profit=9) THROW 53004,'Historical COGS did not use the sale cost snapshot.',1;
IF OBJECT_ID(N'dbo.vw_InventoryLosses',N'V') IS NULL OR OBJECT_ID(N'dbo.vw_ExpiringBatches',N'V') IS NULL OR OBJECT_ID(N'dbo.vw_ReturnAnalysis',N'V') IS NULL THROW 53005,'Required operational report views are missing.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.ActionPermissions WHERE role_name=N'cashier' AND action_key=N'SALE.CREATE' AND is_allowed=1) THROW 53006,'Cashier sale permission missing.',1;
DECLARE @before DECIMAL(24,8)=(SELECT quantity FROM dbo.ProductStock WHERE warehouse_id=@warehouse AND product_id=@product);
BEGIN TRY UPDATE dbo.ProductStock SET quantity=quantity-100 WHERE warehouse_id=@warehouse AND product_id=@product;THROW 53007,'Negative stock was accepted.',1;END TRY BEGIN CATCH IF ERROR_NUMBER()=53007 THROW;END CATCH;
IF (SELECT quantity FROM dbo.ProductStock WHERE warehouse_id=@warehouse AND product_id=@product)<>@before THROW 53008,'Failed stock mutation was not rolled back.',1;
ROLLBACK TRANSACTION;
"@
NonQuery $behavior $fresh
Write-Host "Database validation passed: fresh install, ordered migrations, preserved upgrade data, constraints, rollback and snapshot reporting." -ForegroundColor Green
