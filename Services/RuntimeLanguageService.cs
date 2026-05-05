using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class RuntimeLanguageService
    {
        private sealed class Translation
        {
            public Translation(string english, string arabic)
            {
                English = english;
                Arabic = arabic;
            }

            public string English { get; }
            public string Arabic { get; }
        }

        private static readonly List<Translation> translations = new List<Translation>
        {
            Add("Settings", "الإعدادات"),
            Add("Date", "التاريخ"),
            Add("Bike", "Bike"),
            Add("Zone", "Zone"),
            Add("BIKE", "Bike"),
            Add("ZONE", "Zone"),
            Add("English", "إنكليزي"),
            Add("Arabic", "عربي"),
            Add("Daily", "يومي"),
            Add("Weekly", "أسبوعي"),
            Add("Monthly", "شهري"),
            Add("Manual", "يدوي"),
            Add("Screen", "الشاشة"),
            Add("Refresh", "تحديث"),
            Add("Back", "رجوع"),
            Add("Skip", "تخطي"),
            Add("Save As", "حفظ باسم"),
            Add("General Settings", "الإعدادات العامة"),
            Add("User & Role Settings", "إعدادات المستخدمين والصلاحيات"),
            Add("Sales & Transactions", "المبيعات والمعاملات"),
            Add("Inventory & Reports", "المخزون والتقارير"),
            Add("Security & Access", "الأمان والوصول"),
            Add("Currency:", "العملة:"),
            Add("Theme:", "المظهر:"),
            Add("Language:", "اللغة:"),
            Add("Default", "افتراضي"),
            Add("Light", "فاتح"),
            Add("Dark", "داكن"),
            Add("Default Role:", "الصلاحية الافتراضية:"),
            Add("Password Policy:", "سياسة كلمة المرور:"),
            Add("Max Length:", "الحد الأقصى:"),
            Add("Session Timeout:", "مهلة الجلسة:"),
            Add("Admin", "مدير"),
            Add("Manager", "مسؤول"),
            Add("Cashier", "كاشير"),
            Add("Casheir", "كاشير"),
            Add("Tax Percentage:", "نسبة الضريبة:"),
            Add("Max Discount Allowed:", "أقصى خصم مسموح:"),
            Add("Dollar Exchange Rate", "سعر صرف الدولار"),
            Add("سعر صرف الدولار", "سعر صرف الدولار"),
            Add("Low Stock Alert:", "تنبيه انخفاض المخزون:"),
            Add("Report Format:", "صيغة التقرير:"),
            Add("Backup Schedule:", "جدولة النسخ الاحتياطي:"),
            Add("Items", "أصناف"),
            Add("Enable Audit Logs", "تفعيل سجل التدقيق"),
            Add("Manager Approval for Refunds", "موافقة المسؤول على المرتجعات"),
            Add("Lock System After Faild Logins:", "قفل النظام بعد محاولات فاشلة:"),
            Add("Attempts", "محاولات"),
            Add("Save Changes", "حفظ التعديلات"),
            Add("Reset Defaults", "استعادة الافتراضي"),
            Add("Close", "إغلاق"),

            Add("Sales", "المبيعات"),
            Add("Start Sales Work", "بدء عمل المبيعات"),
            Add("START", "ابدأ"),
            Add("Search", "بحث"),
            Add("Search by name product or barcode", "بحث باسم المنتج أو الباركود"),
            Add("Clear", "مسح"),
            Add("Excel", "إكسل"),
            Add("Add", "إضافة"),
            Add("Edit", "تعديل"),
            Add("Delete", "حذف"),
            Add("Save", "حفظ"),
            Add("Cancel", "إلغاء"),
            Add("Print", "طباعة"),
            Add("Logout", "تسجيل الخروج"),
            Add("Dashboard", "لوحة التحكم"),
            Add("Inventory", "المخزون"),
            Add("Add Sales", "إضافة مبيعات"),
            Add("Customers", "الزبائن"),
            Add("Earning Report", "تقرير الأرباح"),
            Add("Warehouse Reports", "تقارير المستودع"),
            Add("Reports", "التقارير"),
            Add("Users", "المستخدمون"),
            Add("Products", "المنتجات"),
            Add("Categories", "الأصناف"),
            Add("Suppliers", "الموردون"),
            Add("Customer Info", "معلومات الزبون"),
            Add("Received Amount", "المبلغ المقبوض"),
            Add("Change", "الباقي"),
            Add("Cash", "نقداً"),
            Add("Credit Card", "بطاقة ائتمان"),
            Add("Digital Wallet", "محفظة رقمية"),
            Add("Print Receipt", "طباعة الإيصال"),

            Add("Add New Products", "إضافة منتج جديد"),
            Add("Product ID", "رقم المنتج"),
            Add("Product Name", "اسم المنتج"),
            Add("Product", "المنتج"),
            Add("Add Product", "إضافة منتج"),
            Add("Add New Product", "إضافة منتج جديد"),
            Add("Update Product", "تعديل المنتج"),
            Add("Barcode", "الباركود"),
            Add("Category", "الصنف"),
            Add("Supplier", "المورد"),
            Add("Quantity", "الكمية"),
            Add("Stock", "المخزون"),
            Add("Price USD", "السعر $"),
            Add("Price LB", "السعر ل.ل"),
            Add("Total", "الإجمالي"),
            Add("Total Dollar:", "إجمالي الدولار:"),
            Add("Total Lebanon:", "إجمالي الليرة:"),
            Add("Dollar", "دولار"),
            Add("Lebanon", "ليرة"),
            Add("Payment Method:", "طريقة الدفع:"),
            Add("Customer:", "الزبون:"),
            Add("Paid Amount", "المبلغ المدفوع"),
            Add("Date/Time", "التاريخ والوقت"),
            Add("Balance", "الرصيد"),
            Add("Return", "مرتجع"),
            Add("Payment", "الدفع"),
            Add("Active", "نشط"),
            Add("Returned", "مرتجع"),

            Add("Earning Reports", "تقارير الأرباح"),
            Add("Total Sales", "إجمالي المبيعات"),
            Add("Profit", "الربح"),
            Add("Top", "الأعلى"),
            Add("Report Type", "نوع التقرير"),
            Add("All sellers", "كل البائعين"),
            Add("Details", "تفصيلي"),
            Add("By Seller", "حسب البائع"),
            Add("By Product", "حسب المنتج"),
            Add("By Day", "حسب اليوم"),
            Add("Invoice", "الفاتورة"),
            Add("Seller", "البائع"),
            Add("Customer", "الزبون"),
            Add("Sales USD", "المبيعات $"),
            Add("Sales LBP", "المبيعات ل.ل"),
            Add("Profit USD", "الربح $"),
            Add("Profit LBP", "الربح ل.ل"),
            Add("Margin %", "هامش الربح %"),

            Add("Work History", "سجل العمل"),
            Add("History", "السجل"),
            Add("Generated By", "أُنشئ بواسطة"),
            Add("Created At", "تاريخ الإنشاء"),
            Add("File Path", "مسار الملف"),
            Add("From Date", "من تاريخ"),
            Add("To Date", "إلى تاريخ"),
            Add("User Name", "اسم المستخدم"),
            Add("Work Date", "تاريخ العمل"),
            Add("Start Time", "وقت البدء"),
            Add("End Time", "وقت الانتهاء"),
            Add("Hours", "ساعات"),
            Add("Minutes", "دقائق"),
            Add("Status", "الحالة"),
            Add("Open", "مفتوح"),
            Add("Closed", "مغلق"),

            Add("UserName:", "اسم المستخدم:"),
            Add("Username", "اسم المستخدم"),
            Add("Password:", "كلمة المرور:"),
            Add("Confirm Password:", "تأكيد كلمة المرور:"),
            Add("Phone", "الهاتف"),
            Add("Email", "البريد الإلكتروني"),
            Add("Address", "العنوان"),
            Add("Name", "الاسم"),
            Add("Description", "الوصف"),
            Add("Add New Category", "إضافة صنف جديد"),
            Add("Add New Supplier", "إضافة مورد جديد"),
            Add("Warehouse", "المستودع"),
            Add("Purchase Cost", "تكلفة الشراء"),
            Add("Capital Value", "قيمة رأس المال"),
            Add("Number Of Items", "عدد الأصناف"),
            Add("Total Units", "إجمالي الوحدات"),
            Add("ID", "الرقم"),
            Add("Record", "السجل"),
            Add("Action", "الإجراء"),
            Add("Details", "التفاصيل"),
            Add("Time", "الوقت"),
            Add("Day", "اليوم"),
            Add("Amount", "المبلغ"),
            Add("Cost USD", "الكلفة $"),
            Add("Cost LBP", "الكلفة ل.ل"),
            Add("Sold Unit Price", "سعر البيع للوحدة"),
            Add("Avg Price", "متوسط السعر"),
            Add("Items Sold", "الكمية المباعة"),
            Add("Invoices", "الفواتير"),
            Add("Invoice ID", "رقم الفاتورة"),
            Add("Sale Date", "تاريخ البيع"),
            Add("Report Type", "نوع التقرير")
        };

        private static readonly Dictionary<string, Translation> lookup = BuildLookup();
        private static readonly Dictionary<Form, bool> formLanguages = new Dictionary<Form, bool>();

        public static void Apply(Form form, bool isArabic)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            formLanguages[form] = isArabic;
            form.Disposed -= Form_Disposed;
            form.Disposed += Form_Disposed;
            TranslateControl(form, isArabic);
        }

        private static void TranslateControl(Control control, bool isArabic)
        {
            if (control == null)
            {
                return;
            }

            if (ShouldTranslateText(control))
            {
                control.Text = Translate(control.Text, isArabic);
            }

            if (control is DataGridView grid)
            {
                foreach (DataGridViewColumn column in grid.Columns.Cast<DataGridViewColumn>().ToList())
                {
                    column.HeaderText = Translate(column.HeaderText, isArabic);
                    if (column is DataGridViewButtonColumn buttonColumn)
                    {
                        buttonColumn.Text = Translate(buttonColumn.Text, isArabic);
                    }
                }
            }

            if (control is ToolStrip toolStrip)
            {
                TranslateToolStripItems(toolStrip.Items, isArabic);
            }

            control.ControlAdded -= Control_ControlAdded;
            control.ControlAdded += Control_ControlAdded;

            foreach (Control child in control.Controls.Cast<Control>().ToList())
            {
                TranslateControl(child, isArabic);
            }
        }

        private static void Control_ControlAdded(object sender, ControlEventArgs e)
        {
            Form form = (sender as Control)?.FindForm();
            if (form == null || !formLanguages.TryGetValue(form, out bool isArabic))
            {
                return;
            }

            TranslateControl(e.Control, isArabic);
        }

        private static void Form_Disposed(object sender, EventArgs e)
        {
            if (sender is Form form)
            {
                formLanguages.Remove(form);
            }
        }

        private static void TranslateToolStripItems(ToolStripItemCollection items, bool isArabic)
        {
            foreach (ToolStripItem item in items.Cast<ToolStripItem>().ToList())
            {
                item.Text = Translate(item.Text, isArabic);

                if (item is ToolStripDropDownItem dropDownItem)
                {
                    TranslateToolStripItems(dropDownItem.DropDownItems, isArabic);
                }
            }
        }

        private static bool ShouldTranslateText(Control control)
        {
            return control is Form ||
                   control is Label ||
                   control is Button ||
                   control is GroupBox ||
                   control is CheckBox ||
                   control is RadioButton;
        }

        private static string Translate(string text, bool isArabic)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string prefix;
            string core;
            string suffix;
            SplitText(text, out prefix, out core, out suffix);

            string key = Normalize(core);
            if (!lookup.TryGetValue(key, out Translation translation))
            {
                return RemoveDecorativeColons(text);
            }

            return RemoveDecorativeColons(prefix + (isArabic ? translation.Arabic : translation.English) + suffix);
        }

        private static void SplitText(string text, out string prefix, out string core, out string suffix)
        {
            int start = 0;
            int end = text.Length - 1;

            while (start <= end && (char.IsWhiteSpace(text[start]) || text[start] == ':' || text[start] == '-'))
            {
                start++;
            }

            while (end >= start && (char.IsWhiteSpace(text[end]) || text[end] == ':' || text[end] == '-'))
            {
                end--;
            }

            prefix = text.Substring(0, start);
            core = start <= end ? text.Substring(start, end - start + 1) : string.Empty;
            suffix = end + 1 < text.Length ? text.Substring(end + 1) : string.Empty;
        }

        private static Dictionary<string, Translation> BuildLookup()
        {
            Dictionary<string, Translation> map = new Dictionary<string, Translation>(StringComparer.OrdinalIgnoreCase);

            foreach (Translation translation in translations)
            {
                map[Normalize(translation.English)] = translation;
                map[Normalize(translation.Arabic)] = translation;
            }

            return map;
        }

        private static Translation Add(string english, string arabic)
        {
            return new Translation(english, arabic);
        }

        private static string Normalize(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            normalized = normalized.Replace("_", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = normalized.Trim(':', '-', ' ');
            return normalized;
        }

        private static string RemoveDecorativeColons(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            string cleaned = Regex.Replace(value, @"(?<!\d)[:：]+|[:：]+(?!\d)", string.Empty);
            return Regex.Replace(cleaned, @"\s{2,}", " ").Trim();
        }
    }
}
