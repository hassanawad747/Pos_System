# CODEX MASTER EXECUTION PLAN — POS SYSTEM

## Purpose
This file is the single source of truth for Codex when continuing and finishing the POS System upgrade on the user's Windows computer.

Codex must:
1. Clone the correct repository branch to the user's Desktop.
2. Inspect the current implementation before changing anything.
3. Finish all remaining roadmap items without removing working features.
4. Keep every database change as an ordered SQL migration.
5. Keep the Server/Client LAN installer working and SQL-only; do not re-introduce a `.bak` installer dependency.
6. Build both the production .NET Framework WinForms app and the parallel .NET 10 modernization app.
7. Run database fresh-install/upgrade validation.
8. Build the installer package.
9. Launch the new production desktop application at the end so the user can see the upgraded version.
10. Fix errors and retry until the acceptance criteria in this document pass.

---

# 1. Repository and branch — DO THIS FIRST

Repository:

```text
https://github.com/hassanawad747/Pos_System.git
```

Development branch containing the new work:

```text
agent/pos-foundation-fixes
```

Do NOT start from `master`. The upgrade branch is far ahead of master and contains the new modules, migrations, installer, UI work, reports, security work, and .NET 10 modernization project.

## Clone to Desktop

Open PowerShell and run:

```powershell
$desktop = [Environment]::GetFolderPath('Desktop')
Set-Location $desktop

if (Test-Path "$desktop\Pos_System\.git") {
    Set-Location "$desktop\Pos_System"
    git fetch origin
    git checkout agent/pos-foundation-fixes
    git pull --ff-only origin agent/pos-foundation-fixes
}
else {
    if (Test-Path "$desktop\Pos_System") {
        $backupName = "Pos_System_old_" + (Get-Date -Format 'yyyyMMdd_HHmmss')
        Rename-Item "$desktop\Pos_System" $backupName
    }

    git clone -b agent/pos-foundation-fixes https://github.com/hassanawad747/Pos_System.git "$desktop\Pos_System"
    Set-Location "$desktop\Pos_System"
}

git status
git branch --show-current
git log -1 --oneline
```

Expected branch:

```text
agent/pos-foundation-fixes
```

The working folder must be:

```text
%USERPROFILE%\Desktop\Pos_System
```

---

# 2. Safety rules — NEVER IGNORE THESE

1. Never delete the user's existing production database just to apply an update.
2. Before changing an existing local database, create a backup and verify it when possible.
3. The installer must remain SQL-script based. Do not add `pos_system.bak` back into the installer package.
4. Fresh server installation must use:
   - `database/install/000_base_schema.sql`
   - then every file in `database/migrations/*.sql` in filename order.
5. Existing server upgrade must keep existing business data and only apply missing migrations.
6. Client installation must never create a local business database; it connects to the manager/server PC over LAN.
7. Never store a real Whish/WHISH merchant secret on every client computer.
8. WHISH payments must remain fail-closed until official merchant API credentials and API/webhook documentation are available. Never fake a successful real payment.
9. Preserve backward compatibility while modernizing. Do not perform a destructive full rewrite.
10. Do not merge directly into `master` until the production branch passes all checks and the user intentionally approves the merge.

---

# 3. Understand the current architecture before editing

Production app:

```text
C# WinForms
.NET Framework 4.8
EF Core 3.1 + legacy/raw SQL areas
SQL Server
Pos_System.csproj
```

Parallel modernization path:

```text
modern/PosSystem.Modern/
.NET 10 Windows Forms
SDK-style csproj
Microsoft.Data.SqlClient
Microsoft.Extensions.DependencyInjection
```

Current installer model:

```text
SERVER:
  central SQL Server + pos_system database + migrations

CLIENT:
  desktop application only
  connects to SERVER IP/name over LAN
```

Main installer entry point:

```text
installer/install_pos_system.bat
```

LAN installer engine:

```text
installer/setup_pos_lan.ps1
```

Database verification utility:

```text
installer/verify_database.ps1
```

---

# 4. Baseline validation before more development

From the repository root:

## Restore packages

```powershell
nuget restore packages.config -PackagesDirectory packages
```

If `nuget` is not in PATH, locate `nuget.exe` or install/use NuGet CLI.

## Build current production app

Use a Visual Studio Developer PowerShell/Command Prompt if needed:

```powershell
msbuild Pos_System.csproj /m /p:Configuration=Debug /p:Platform=AnyCPU /verbosity:minimal
```

Then build Release:

```powershell
msbuild Pos_System.csproj /m /p:Configuration=Release /p:Platform=AnyCPU /verbosity:minimal
```

Both must succeed before continuing.

## Build .NET 10 modernization project

```powershell
dotnet --info
dotnet restore .\modern\PosSystem.Modern\PosSystem.Modern.csproj
dotnet build .\modern\PosSystem.Modern\PosSystem.Modern.csproj -c Release
```

If .NET 10 SDK is missing, install .NET 10 SDK before continuing.

---

# 5. Existing modules that must be preserved and verified

Do not recreate these blindly. They already have implementation work on this branch and should be inspected, tested, and improved where needed:

- Purchases + PurchaseItems
- Purchase stock increase
- InventoryTransactions ledger foundation
- Supplier ledger + supplier payments
- Customer ledger + customer payments
- Expenses
- Cash registers
- Cash sessions / shifts
- Cash movements
- X report / Z report
- SalePayments
- Split payments
- CASH / CARD / BANK / OTHER payment paths
- WHISH payment UI/database/audit foundation
- WHISH phone number/provider fields
- WhishPaymentAttempts audit table
- Modern Login UI
- Modern dashboard shell/navigation
- Shared UI theme engine
- Custom application/shortcut icon generation
- SQL-only Server/Client LAN installer
- Database migration runner model
- Held sales / Resume foundation
- Quotations foundation
- Returns/Exchange foundation
- Warehouses / ProductStock
- Stock count
- Stock transfer
- Units and conversion foundation
- Multiple barcodes
- Batches / expiry
- Serials / IMEI
- Reorder settings / stock health
- Pricing / tax / exchange rates administration
- Promotions foundation
- Loyalty foundation
- BI/reporting views
- Profit/COGS reporting foundation
- Action permissions foundation
- Central error logging foundation
- Password legacy-fallback control
- Database backup/verification tool
- Dependency injection composition root foundation
- Parallel .NET 10 modernization project

Before making a replacement, inspect the actual file and preserve working logic.

---

# 6. Remaining functional integration work — FINISH ALL OF THIS

Codex must review each item against the actual current branch. If already fully implemented and tested, mark it complete and move on. Otherwise finish it.

## 6.1 Complete Sales lifecycle integration

### Hold / Resume
- Hold the current cart without creating a completed sale.
- Persist customer, currency, exchange rate, discount/tax context, notes, user, and every cart line.
- Resume into the Sales screen with the exact quantities/prices/discounts restored.
- Completing a resumed sale must mark/remove the held record correctly.
- Prevent duplicate completion.

### Quotation
- Create quotation header + items.
- Support expiry date/status.
- Open/view quotation.
- Convert quotation to a real Sale transaction.
- Conversion must create sale/items/payments/stock movements correctly and mark quotation converted.
- Do not reduce stock when quotation is only saved.

### Return / Exchange
- Support full return.
- Support partial return.
- Prevent returning more than originally sold minus previous returns.
- Support optional restock.
- Create inventory transactions for returned/restocked items.
- Correct customer ledger.
- Correct cash movement/refund behavior.
- Support refund destinations: CASH, CARD, BANK, CUSTOMER_CREDIT, OTHER as appropriate.
- For Exchange: return old items and add replacement sale items in one controlled workflow; calculate amount customer owes or amount to refund.
- Use an atomic SQL transaction.
- Add/verify action permissions for return/exchange/refund.

## 6.2 Finish inventory professionalization

### Stock Count
- Count by warehouse/product.
- Save expected, actual, variance, user, time, reason.
- Create ADJUSTMENT inventory transaction.
- Update ProductStock atomically.

### Warehouse Transfer
- Source warehouse stock decreases.
- Destination warehouse stock increases.
- Create TRANSFER_OUT + TRANSFER_IN transactions.
- Reject insufficient source stock.
- Transaction must be atomic.

### Damage / Expired
- Finish StockLoss workflow if partially implemented.
- DAMAGE and EXPIRED must reduce warehouse stock.
- Save reason/reference/user/date.
- Create inventory transaction.
- Prevent negative stock.
- Include these losses in reports.

### Units
- Piece/Box/Pack/Kg/Gram/Liter/Meter etc.
- Product unit conversion factors.
- Sales should understand selected unit and convert to base-stock quantity.
- Barcode can identify a product unit.

### Multiple barcodes
- Unique barcode enforcement.
- Barcode lookup must search ProductBarcodes in addition to legacy Products.barcode.
- Link barcode to optional ProductUnit.

### Batches / Expiry
- Batch number + product + warehouse + quantity + expiry date.
- Track expiration.
- Provide expiring/expired report.
- When appropriate, sale stock deduction should use a defined batch policy such as FEFO.

### Serial / IMEI
- Unique serial enforcement.
- Track status and sale/reference.
- Prevent selling the same serial twice.

### Reorder
- minimum_stock
- maximum_stock
- reorder_point
- low/reorder alerts in dashboard/reports.

## 6.3 Finish Pricing / Tax / Currency integration into real checkout

Administration tables alone are not enough.

### Currency / Exchange Rate
- Save transaction currency and exchange-rate snapshot on every sale/purchase/payment where required.
- Old invoices must never recalculate using today's rate.
- Validate USD/LBP calculations.

### Tax
- Product/category/default tax resolution.
- Support 0% exempt and multiple tax rates.
- Inclusive/exclusive tax behavior where configured.
- Store tax amount snapshot on transaction/item.

### Promotions
- Apply active date/time rules.
- Invoice threshold discounts.
- Product/category discounts.
- Buy-X/Get-Y only if the schema/logic supports it correctly; otherwise extend schema with a new migration first.
- Avoid double-discount bugs.

### Loyalty
- Earn points from qualifying sales.
- Redeem points with validation.
- Record LoyaltyTransactions.
- Never update only a balance without a transaction trail.

### Wholesale/customer price levels
- Finish ProductPrices / customer price level selection.
- Resolve the correct selling price at checkout.

## 6.4 Finish advanced reports / BI

Verify and complete:
- Revenue
- COGS
- Gross Profit
- Expenses
- Net Profit
- Daily/Weekly/Monthly sales
- Best sellers
- Slow moving stock
- Dead stock
- Stock valuation at cost and retail
- Out-of-stock / low-stock / reorder
- Expired / expiring batches
- Customer balances
- Customer profitability
- Supplier balances
- Supplier performance
- Cashier performance
- Shift differences
- Hourly sales
- Returns analysis
- Discounts analysis
- Audit/user activity

Important: historical COGS must use cost snapshots stored at sale time, not today's product purchase cost.

## 6.5 Security and reliability completion

- Apply ActionPermissions to every sensitive operation:
  - sale create/void
  - discount
  - return/exchange
  - refund
  - purchase create
  - supplier payment
  - customer adjustment
  - stock count/adjustment/transfer/damage/expired
  - user/permissions
  - settings/pricing/tax/currency
- Migrate legacy user passwords naturally on successful login.
- Only disable plaintext/SHA legacy fallback after there are no remaining legacy password hashes.
- Keep PBKDF2 authentication.
- Keep failed-login/session timeout controls working.
- Centralize exception logging into ErrorLogs.
- Do not expose database passwords or merchant secrets in logs.
- Prevent double-submit for sale/purchase/payment/return workflows.
- Keep concurrency controls for the same stock item on multiple LAN clients.
- Backup + verification tool must work on the SERVER without adding a `.bak` to the installer repository.

## 6.6 Dependency security

Review NuGet vulnerability warnings.

At minimum inspect/update safely where compatible:
- Microsoft.Data.SqlClient legacy dependency used by the old app
- Newtonsoft.Json legacy version
- IdentityModel/JWT packages if still required

Do not blindly upgrade a package if it breaks .NET Framework 4.8. Prefer tested incremental upgrades and use the .NET 10 project for modern dependencies.

---

# 7. Complete architecture modernization safely

The production app must remain functional while modernization proceeds.

## Current production architecture target

Move new code progressively toward:

```text
Forms
  -> Application Services
  -> Domain / Validators
  -> Repositories / Data Access
  -> SQL Server / EF
```

Requirements:
- Forms should not contain large SQL/business workflows.
- New business rules go into services/domain classes.
- Use DI composition root (`AppServices`) instead of creating every service directly inside Forms.
- Introduce repository abstractions where they add testability/value.
- Keep SQL transactions in the service/repository layer.

## .NET 10 modernization project

Location:

```text
modern/PosSystem.Modern/
```

Codex must continue feature-parity work there without replacing the production executable before it is ready.

Target:

```text
net10.0-windows
```

Steps:
1. Keep project SDK-style.
2. Use PackageReference.
3. Use supported Microsoft.Data.SqlClient.
4. Use DI.
5. Reuse the same central SQL Server schema/migrations.
6. Port shell/navigation first.
7. Port login/security.
8. Port Sales checkout and payments.
9. Port inventory/purchases/ledgers.
10. Port reporting/admin modules.
11. Add automated comparison tests before switching production installer to the modern executable.

Do not switch installer default executable from the .NET Framework production app to the .NET 10 app until feature parity is verified.

---

# 8. Automated tests / validation Codex must add or verify

At minimum cover:

1. Invoice totals.
2. Discounts.
3. Tax inclusive/exclusive calculations.
4. USD/LBP exchange conversion.
5. Exchange-rate snapshot behavior.
6. Stock deduction on sale.
7. Stock increase on purchase.
8. Stock count adjustment.
9. Warehouse transfer.
10. Damage/expired deduction.
11. Return and exchange.
12. Customer ledger.
13. Supplier ledger.
14. Customer payment.
15. Supplier payment.
16. Expense cash movement.
17. Cash session expected balance.
18. Split payments.
19. Cash + Card mixed payment.
20. WHISH fail-closed behavior without provider configuration.
21. Permissions.
22. Password authentication.
23. Legacy password migration.
24. Rollback on failure.
25. Double-submit protection.
26. Concurrent sales of the same product.
27. Unique barcode.
28. Unique serial/IMEI.
29. Quotation does not deduct stock.
30. Quotation conversion deducts stock once.
31. Partial return cannot exceed sold quantity.
32. Profit/COGS reporting uses cost snapshot.

CI must continue to run fresh SQL installation + all migrations in order.

---

# 9. Database migration rules for every new change

For every schema change:

1. Create a new file; never silently edit an already-deployed migration unless it has definitely never been released anywhere.
2. Filename format:

```text
018_feature_name.sql
019_next_feature.sql
...
```

3. Make migration idempotent where practical.
4. Register migration in `dbo.SchemaMigrations`.
5. Use transactions and `SET XACT_ABORT ON` where appropriate.
6. Preserve existing data.
7. Update CI schema verification.
8. Installer automatically packages every `database/migrations/*.sql` file; keep this behavior.

---

# 10. Installer requirements — MUST REMAIN WORKING

The final installer must offer:

```text
1 - SERVER
2 - CLIENT
```

## SERVER

SERVER mode must:
- Detect/install/configure SQL Server Express if required.
- Enable SQL authentication as designed.
- Enable TCP/IP.
- Use configured LAN SQL port (currently 1433 unless config changes intentionally).
- Open required Windows Firewall rule.
- Ask for database password securely.
- Create/use the application SQL login.
- Create database from `000_base_schema.sql` only on fresh installation.
- Run every migration in order.
- Preserve an existing database on upgrade.
- Create first POS administrator account for a fresh system.
- Install application files.
- Generate `Database.config`.
- Generate the custom BikeZonePOS icon.
- Create desktop shortcut.
- Display server computer name/IP so clients can connect.

## CLIENT

CLIENT mode must:
- Ask for Server PC name or LAN IP.
- Ask for database password.
- Test TCP port/database login.
- Never create a local business database.
- Install the desktop app.
- Write `Database.config` using the server address.
- Create branded shortcut.
- Launch the application.

## No backup file in installer

Do NOT add this back:

```text
installer/Database/pos_system.bak
```

Backup verification is an operational SERVER function, not the installer baseline.

---

# 11. Build the final production release

From:

```text
%USERPROFILE%\Desktop\Pos_System
```

Run:

```powershell
nuget restore packages.config -PackagesDirectory packages
msbuild Pos_System.csproj /m /p:Configuration=Release /p:Platform=AnyCPU /verbosity:minimal
```

Expected production executable:

```text
bin\Release\Pos_System.exe
```

Also build modern project:

```powershell
dotnet build .\modern\PosSystem.Modern\PosSystem.Modern.csproj -c Release
```

Both builds must succeed.

---

# 12. Run database verification before launching

If this computer is the SERVER and its database is already configured, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\verify_database.ps1
```

If the database is not installed/configured yet, use the installer:

```powershell
.\installer\install_pos_system.bat
```

Choose `SERVER` on the manager/database computer.

For another LAN workstation choose `CLIENT` and enter the server IP/name and the same database login password requested by the installer.

Never destroy an existing database merely to make local testing easier.

---

# 13. Build/test the installer package locally

Verify these files are included:

```text
App\
Database\000_base_schema.sql
Migrations\*.sql
install_pos_system.bat
setup_pos_lan.ps1
create_pos_icon.ps1
verify_database.ps1
Installer.config.json
README.txt
INSTALL_STEPS.txt
تعليمات_التثبيت_بالعربي.txt
VERSION.txt
```

The package must not depend on a `.bak` file.

If GitHub Actions is available, push the final commits to:

```text
agent/pos-foundation-fixes
```

and verify the `POS Build Validation` workflow passes all steps and produces:

```text
BikeZonePOS-Installer
```

---

# 14. FINAL STEP — RUN THE NEW APPLICATION FOR THE USER

Codex must not stop after saying "build succeeded".

After all required checks pass, launch the production Release executable:

```powershell
$repo = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Pos_System'
$exe = Join-Path $repo 'bin\Release\Pos_System.exe'

if (-not (Test-Path $exe)) {
    throw "Release executable was not found: $exe"
}

Start-Process $exe
```

If the Release executable needs `Database.config`, make sure the correct configuration file is present next to the executable or use the installer-installed application path.

If the installer was used, prefer launching the installed app/desktop shortcut created by the installer.

The user must be able to see the new Login screen and then the new Dashboard after logging in.

Also optionally launch the .NET 10 modernization shell separately for validation, but do not present it as the production-complete app until feature parity is achieved:

```powershell
dotnet run --project .\modern\PosSystem.Modern\PosSystem.Modern.csproj -c Release
```

---

# 15. Final acceptance checklist — CODEX MAY ONLY DECLARE COMPLETE WHEN TRUE

## Repository
- [ ] Project exists at `%USERPROFILE%\Desktop\Pos_System`.
- [ ] Current branch is `agent/pos-foundation-fixes`.
- [ ] Working tree has no accidental/uncommitted junk.

## Production build
- [ ] NuGet restore succeeds.
- [ ] Debug build succeeds.
- [ ] Release build succeeds.
- [ ] `bin\Release\Pos_System.exe` exists.

## Modern build
- [ ] .NET 10 SDK available.
- [ ] Modern project restore succeeds.
- [ ] Modern project Release build succeeds.

## Database
- [ ] Fresh SQL-only install succeeds.
- [ ] All migrations run in filename order.
- [ ] Existing DB upgrade preserves data.
- [ ] `SchemaMigrations` matches repository migration count.
- [ ] DB verification passes.

## Functional
- [ ] Purchases work.
- [ ] Inventory ledger works.
- [ ] Customer ledger works.
- [ ] Supplier ledger works.
- [ ] Expenses work.
- [ ] Cash shifts work.
- [ ] X/Z reports work.
- [ ] Split payment works.
- [ ] WHISH remains fail-closed unless real merchant provider is configured.
- [ ] Hold/Resume works end-to-end.
- [ ] Quotation -> Sale works end-to-end.
- [ ] Partial/full Return works.
- [ ] Exchange works.
- [ ] Stock Count works.
- [ ] Warehouse Transfer works.
- [ ] Damage/Expired workflow works.
- [ ] Units/conversions affect inventory correctly.
- [ ] Multiple barcode lookup works.
- [ ] Batch/expiry tracking works.
- [ ] Serial/IMEI tracking works.
- [ ] Tax/currency/promotions/loyalty are integrated into checkout, not only admin screens.
- [ ] BI/profit reports are accurate.
- [ ] Sensitive actions enforce action permissions.
- [ ] Error logging works.
- [ ] Backup verification works on SERVER.

## Installer
- [ ] SERVER option works.
- [ ] CLIENT option works.
- [ ] No `.bak` installer dependency.
- [ ] Desktop shortcut uses branded icon.
- [ ] Installer contains every migration.

## Final demonstration
- [ ] Production application is launched after successful Release build/install.
- [ ] New Login UI is visible.
- [ ] User can log in and see the upgraded Dashboard.

---

# 16. What Codex should report at the end

Return a concise final report containing:

1. Desktop clone path.
2. Git branch and final commit SHA.
3. Files/modules changed.
4. Migrations added.
5. Production Debug/Release build result.
6. .NET 10 build result.
7. Database validation result.
8. Installer validation result.
9. GitHub Actions result if pushed.
10. Exact executable/shortcut launched.
11. Any external blocker that cannot be solved from code alone.

The known example of an external blocker is real WHISH merchant charging: it requires official Whish merchant credentials/API/webhook contract. Do not claim live payment is finished without those credentials and successful sandbox/production validation.

---

# NON-STOP EXECUTION RULE

Work through the plan continuously. Do not stop after implementing one module. When a build, migration, test, or runtime error occurs:

1. Read the exact error.
2. Fix the root cause.
3. Rebuild/retest.
4. Continue to the next checklist item.

Only stop for a user secret/credential or a genuinely external requirement that cannot safely be invented. Otherwise keep progressing until the final acceptance checklist is satisfied and the production application has been launched.
