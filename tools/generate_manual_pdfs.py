from pathlib import Path
import sys

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfgen import canvas

sys.path.insert(0, str(Path(__file__).parent))
from generate_user_manuals import english_sections, arabic_sections, screen_rows_en, screen_rows_ar


DESKTOP = Path.home() / "Desktop"
EN_PDF = DESKTOP / "POS_System_User_Manual_English.pdf"
AR_PDF = DESKTOP / "POS_System_User_Manual_Arabic.pdf"
FONT_REG = "SegoeUI"
FONT_BOLD = "SegoeUIBold"
FONT_AR = "Tahoma"
FONT_AR_BOLD = "TahomaBold"

pdfmetrics.registerFont(TTFont(FONT_REG, r"C:\Windows\Fonts\segoeui.ttf"))
pdfmetrics.registerFont(TTFont(FONT_BOLD, r"C:\Windows\Fonts\segoeuib.ttf"))
pdfmetrics.registerFont(TTFont(FONT_AR, r"C:\Windows\Fonts\tahoma.ttf"))
pdfmetrics.registerFont(TTFont(FONT_AR_BOLD, r"C:\Windows\Fonts\tahomabd.ttf"))


ARABIC_FORMS = {
    "ء": ("\ufe80", None, None, None),
    "آ": ("\ufe81", "\ufe82", None, None),
    "أ": ("\ufe83", "\ufe84", None, None),
    "ؤ": ("\ufe85", "\ufe86", None, None),
    "إ": ("\ufe87", "\ufe88", None, None),
    "ئ": ("\ufe89", "\ufe8a", "\ufe8b", "\ufe8c"),
    "ا": ("\ufe8d", "\ufe8e", None, None),
    "ب": ("\ufe8f", "\ufe90", "\ufe91", "\ufe92"),
    "ة": ("\ufe93", "\ufe94", None, None),
    "ت": ("\ufe95", "\ufe96", "\ufe97", "\ufe98"),
    "ث": ("\ufe99", "\ufe9a", "\ufe9b", "\ufe9c"),
    "ج": ("\ufe9d", "\ufe9e", "\ufe9f", "\ufea0"),
    "ح": ("\ufea1", "\ufea2", "\ufea3", "\ufea4"),
    "خ": ("\ufea5", "\ufea6", "\ufea7", "\ufea8"),
    "د": ("\ufea9", "\ufeaa", None, None),
    "ذ": ("\ufeab", "\ufeac", None, None),
    "ر": ("\ufead", "\ufeae", None, None),
    "ز": ("\ufeaf", "\ufeb0", None, None),
    "س": ("\ufeb1", "\ufeb2", "\ufeb3", "\ufeb4"),
    "ش": ("\ufeb5", "\ufeb6", "\ufeb7", "\ufeb8"),
    "ص": ("\ufeb9", "\ufeba", "\ufebb", "\ufebc"),
    "ض": ("\ufebd", "\ufebe", "\ufebf", "\ufec0"),
    "ط": ("\ufec1", "\ufec2", "\ufec3", "\ufec4"),
    "ظ": ("\ufec5", "\ufec6", "\ufec7", "\ufec8"),
    "ع": ("\ufec9", "\ufeca", "\ufecb", "\ufecc"),
    "غ": ("\ufecd", "\ufece", "\ufecf", "\ufed0"),
    "ف": ("\ufed1", "\ufed2", "\ufed3", "\ufed4"),
    "ق": ("\ufed5", "\ufed6", "\ufed7", "\ufed8"),
    "ك": ("\ufed9", "\ufeda", "\ufedb", "\ufedc"),
    "ل": ("\ufedd", "\ufede", "\ufedf", "\ufee0"),
    "م": ("\ufee1", "\ufee2", "\ufee3", "\ufee4"),
    "ن": ("\ufee5", "\ufee6", "\ufee7", "\ufee8"),
    "ه": ("\ufee9", "\ufeea", "\ufeeb", "\ufeec"),
    "و": ("\ufeed", "\ufeee", None, None),
    "ى": ("\ufeef", "\ufef0", None, None),
    "ي": ("\ufef1", "\ufef2", "\ufef3", "\ufef4"),
}


def connects_next(ch):
    forms = ARABIC_FORMS.get(ch)
    return bool(forms and forms[2])


def connects_prev(ch):
    forms = ARABIC_FORMS.get(ch)
    return bool(forms and forms[1])


def shape_arabic(text):
    chars = list(text)
    shaped = []
    for i, ch in enumerate(chars):
        forms = ARABIC_FORMS.get(ch)
        if not forms:
            shaped.append(ch)
            continue
        prev_ch = chars[i - 1] if i > 0 else ""
        next_ch = chars[i + 1] if i + 1 < len(chars) else ""
        join_prev = connects_next(prev_ch) and connects_prev(ch)
        join_next = connects_next(ch) and connects_prev(next_ch)
        if join_prev and join_next and forms[3]:
            shaped.append(forms[3])
        elif join_prev and forms[1]:
            shaped.append(forms[1])
        elif join_next and forms[2]:
            shaped.append(forms[2])
        else:
            shaped.append(forms[0])
    return "".join(shaped)


def visual_ar(text):
    return shape_arabic(text)[::-1]


class ManualPdf:
    def __init__(self, path, rtl=False):
        self.c = canvas.Canvas(str(path), pagesize=A4)
        self.width, self.height = A4
        self.margin = 42
        self.y = self.height - self.margin
        self.rtl = rtl
        self.page = 1
        self.font = FONT_AR if rtl else FONT_REG
        self.bold = FONT_AR_BOLD if rtl else FONT_BOLD

    def new_page(self):
        self.footer()
        self.c.showPage()
        self.page += 1
        self.y = self.height - self.margin

    def ensure(self, amount):
        if self.y - amount < self.margin + 22:
            self.new_page()

    def footer(self):
        self.c.setFont(self.font, 8)
        self.c.setFillColor(colors.HexColor("#64748b"))
        text = f"POS System User Manual | Page {self.page}" if not self.rtl else f"دليل نظام نقاط البيع | صفحة {self.page}"
        if self.rtl:
            self.c.drawRightString(self.width - self.margin, 24, visual_ar(text))
        else:
            self.c.drawCentredString(self.width / 2, 24, text)
        self.c.setFillColor(colors.black)

    def draw_text(self, text, size=10.5, bold=False, indent=0, color="#111827", spacing=5):
        font = self.bold if bold else self.font
        self.c.setFont(font, size)
        self.c.setFillColor(colors.HexColor(color))
        max_width = self.width - self.margin * 2 - indent
        lines = self.wrap(text, font, size, max_width)
        for line in lines:
            self.ensure(size + spacing)
            if self.rtl:
                self.c.drawRightString(self.width - self.margin - indent, self.y, visual_ar(line))
            else:
                self.c.drawString(self.margin + indent, self.y, line)
            self.y -= size + spacing
        self.c.setFillColor(colors.black)

    def wrap(self, text, font, size, max_width):
        words = str(text).split()
        lines = []
        current = ""
        for word in words:
            test = word if not current else current + " " + word
            measure = visual_ar(test) if self.rtl else test
            if pdfmetrics.stringWidth(measure, font, size) <= max_width:
                current = test
            else:
                if current:
                    lines.append(current)
                current = word
        if current:
            lines.append(current)
        return lines or [""]

    def heading(self, text, level=1):
        size = 18 if level == 1 else 14
        self.ensure(size + 18)
        self.y -= 6
        self.draw_text(text, size=size, bold=True, color="#0f172a", spacing=7)

    def bullet(self, text):
        bullet = "• "
        self.draw_text((bullet + text) if not self.rtl else (text + " •"), indent=14, spacing=4)

    def table(self, headers, rows):
        col1 = 155
        col2 = self.width - self.margin * 2 - col1
        row_h = 28
        self.ensure(row_h * (len(rows) + 2))
        x = self.margin
        for idx, row in enumerate([headers] + rows):
            bg = colors.HexColor("#e2e8f0") if idx == 0 else colors.white
            self.c.setFillColor(bg)
            self.c.rect(x, self.y - row_h + 8, col1 + col2, row_h, fill=1, stroke=1)
            self.c.setFillColor(colors.black)
            self.c.line(x + col1, self.y - row_h + 8, x + col1, self.y + 8)
            self.c.setFont(self.bold if idx == 0 else self.font, 9)
            first, second = row
            if self.rtl:
                self.c.drawRightString(x + col1 - 8, self.y - 11, visual_ar(first))
                self.c.drawRightString(x + col1 + col2 - 8, self.y - 11, visual_ar(second[:80]))
            else:
                self.c.drawString(x + 8, self.y - 11, first)
                self.c.drawString(x + col1 + 8, self.y - 11, second[:80])
            self.y -= row_h
        self.y -= 10

    def save(self):
        self.footer()
        self.c.save()


def build_pdf(path, title, subtitle, sections, screen_rows, rtl=False):
    pdf = ManualPdf(path, rtl)
    pdf.draw_text(title, size=24, bold=True, color="#0f172a", spacing=10)
    pdf.draw_text(subtitle, size=13, color="#475569", spacing=16)
    creator_lines = [
        "Created by Hassan Awwad",
        "Phone: +96170062834",
        "Email: hassanawod12346@gmail.com",
    ]
    if rtl:
        pdf.c.setFont(pdf.bold, 11)
        pdf.c.setFillColor(colors.HexColor("#0f766e"))
        for line in creator_lines:
            pdf.ensure(16)
            pdf.c.drawRightString(pdf.width - pdf.margin, pdf.y, line)
            pdf.y -= 15
        pdf.c.setFillColor(colors.black)
    else:
        for line in creator_lines:
            pdf.draw_text(line, size=11, bold=True, color="#0f766e", spacing=4)
    pdf.y -= 8
    pdf.heading("Quick Screen Guide" if not rtl else "دليل سريع للشاشات")
    pdf.table(["Screen", "Purpose"] if not rtl else ["الشاشة", "الاستخدام"], screen_rows)
    pdf.heading("Contents" if not rtl else "المحتويات")
    for heading, _ in sections:
        pdf.bullet(heading)
    pdf.new_page()
    for heading, bullets in sections:
        pdf.heading(heading)
        for item in bullets:
            pdf.bullet(item)
    pdf.heading("Permission Matrix Summary" if not rtl else "ملخص الصلاحيات")
    matrix = [
        ("View", "Open and see the screen") if not rtl else ("View", "فتح الشاشة ورؤيتها"),
        ("Create", "Use add/new actions") if not rtl else ("Create", "استخدام أزرار الإضافة"),
        ("Edit", "Use edit/update actions") if not rtl else ("Edit", "استخدام أزرار التعديل"),
        ("Save", "Use save/balance/settings actions") if not rtl else ("Save", "استخدام الحفظ وتعديل الأرصدة والإعدادات"),
        ("Delete", "Use delete actions") if not rtl else ("Delete", "استخدام الحذف"),
    ]
    pdf.table(["Permission", "Meaning"] if not rtl else ["الصلاحية", "المعنى"], matrix)
    pdf.save()


build_pdf(
    EN_PDF,
    "POS System User Manual",
    "Complete guide for using the POS application",
    english_sections,
    screen_rows_en,
    rtl=False,
)

build_pdf(
    AR_PDF,
    "دليل استخدام نظام نقاط البيع",
    "شرح كامل لاستخدام التطبيق",
    arabic_sections,
    screen_rows_ar,
    rtl=True,
)

print(EN_PDF)
print(AR_PDF)
