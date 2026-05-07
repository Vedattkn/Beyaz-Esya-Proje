from pathlib import Path
import unicodedata

lines = [
    "9. Hafta Raporu (Detayli)",
    "",
    "Kapsam: Admin paneli ve guvenlik odakli gelistirmeler, urun/stok yonetimi, servis talepleri kontrolu ve UI modernizasyonu.",
    "Tarih: 05.05.2026",
    "",
    "KPI:",
    "- 100% admin ve guvenlik hedefleri kapanis",
    "- 1 stok yonetimi katmani eklendi",
    "- 3+ UI/UX bolumlerinde premium tasarim",
    "- CSRF korumasi admin aksiyonlarinda etkin",
    "",
    "YoneticI Ozeti:",
    "- Admin paneli urun ekleme/silme/guncelleme akislarinda stok yonetimi tamamlandi.",
    "- Admin uzerindeki kritik POST aksiyonlari CSRF dogrulamasi ile guclendirildi.",
    "- Hizmetler ve ana sayfa bolumleri modern CTA ve premium kart tasarimi ile yenilendi.",
    "- Trust bar, ikonlar, hero ve footer daha profesyonel ve satis odakli hale getirildi.",
    "",
    "Plana Gore Tamamlananlar:",
    "- Admin rol kontrolu ve servis talepleri yonetimi (mevcut altyapi uzerinden guclendirildi).",
    "- Urun ekleme/silme/guncelleme akislarina stok alanlari eklendi.",
    "- Stok yonetimi (model + DB self-heal + admin form) tamamlandi.",
    "- Guvenlik onlemleri (CSRF dogrulamalari) tamamlandi.",
    "",
    "Ekstra Kazanimlar:",
    "- Hizmetler sayfasina featured kart + rozet + CTA bolumu eklendi.",
    "- Ana sayfa hero ve trust bar modernlestirildi, yeni aksan renkleri uygulandi.",
    "- Footer 3 kolonlu, ikonlu ve koyu arka planli hale getirildi.",
    "- Marka kartlari ve servis kartlari premium hover/gradient gorsellige kavustu.",
    "",
    "Kronolojik Akis:",
    "- Ana sayfa hero arka plan, overlay ve CTA iyilestirmeleri yapildi.",
    "- Trust bar ikonlari ve kisa metinler guclendirildi.",
    "- Hangi Markalar bolumu premium kart tasarimiyla yenilendi.",
    "- Hizmetler sayfasinda featured kart, hover ve ikon tasarimi guncellendi.",
    "- Hizmetler altina CTA bolumu eklendi.",
    "- Footer 3 kolonlu yapiya tasindi ve koyu tema verildi.",
    "- Urun modeline stok alani eklendi ve Supabase DB self-heal ile desteklendi.",
    "- Admin urun formlarina stok girisi ve listede stok gorunumu eklendi.",
    "- Admin urun CRUD POST aksiyonlari CSRF korumasi ile guclendirildi.",
    "",
    "Modul Bazli Detay:",
    "- Admin / Urun: stok alani, listede stok gorunumu, formlarda stok input.",
    "- Guvenlik: admin create/update/delete aksiyonlarina CSRF dogrulama.",
    "- UI/UX: hero, trust bar, footer, hizmet kartlari ve CTA yenilendi.",
    "- DB / Supabase: stock kolonu self-heal + CRUD alan entegrasyonu.",
    "",
    "Guncel Is Kurallari:",
    "- Stok degeri negatif olamaz.",
    "- Admin urun aksiyonlari CSRF token ile korunur.",
    "- Hizmetler sayfasinda featured kart onceliklidir.",
    "",
    "Mimari Sonuc:",
    "- Stok alani product modeline entegre edildi ve DB tarafinda kalici hale getirildi.",
    "- Admin aksiyonlari guvenlik acisindan bir ust seviyeye tasindi.",
    "- On yuzde premium hissi guclendiren tutarli bir tasarim dili olustu.",
    "",
    "Dogrulama / Operasyon Notlari:",
    "- Stok kolonu DB tarafinda yoksa otomatik olarak eklenir.",
    "- CSRF dogrulamalarinin etkin olmasi icin formlarda token bulunmalidir.",
    "- Uygulama tasarim guncellemeleri tum sayfalarda tutarli gorunur.",
    "",
    "Genel Sonuc:",
    "- 9. hafta kapsaminda admin paneli ve guvenlik hedefleri tamamlandi.",
    "- Stok yonetimi ve UI modernizasyonu ile operasyonel ve kullanici deneyimi iyilesti.",
]


def escape_pdf(text):
    return text.replace("(", "\\(").replace(")", "\\)").replace("\\", "\\\\")


content_lines = []
line_height = 12
font_size = 10
x = 50
page_height = 842

y = 820
content_lines.append("BT")
content_lines.append(f"/F1 {14} Tf")
content_lines.append(f"{x} {y} Td ({escape_pdf(lines[0])}) Tj")
content_lines.append("ET")
y -= 22

content_lines.append("BT")
content_lines.append(f"/F1 {font_size} Tf")
for idx, line in enumerate(lines[1:], start=1):
    if y < 40:
        break
    content_lines.append(f"{x} {y} Td ({escape_pdf(line)}) Tj")
    content_lines.append("ET")
    y -= 14
    if idx < len(lines):
        content_lines.append("BT")

page1_lines = content_lines.copy()
remaining_lines = lines[(idx + 1):]

page2_lines = []
y = 820
page2_lines.append("BT")
page2_lines.append(f"/F1 {14} Tf")
page2_lines.append(f"{x} {y} Td ({escape_pdf('Devam Eden 9. Hafta Detaylari')}) Tj")
page2_lines.append("ET")
y -= 22
page2_lines.append("BT")
page2_lines.append(f"/F1 {font_size} Tf")
for line in remaining_lines:
    if y < 40:
        break
    page2_lines.append(f"{x} {y} Td ({escape_pdf(line)}) Tj")
    page2_lines.append("ET")
    y -= 14
    if line != remaining_lines[-1]:
        page2_lines.append("BT")

stream1 = "\n".join(page1_lines) + "\n"
stream2 = "\n".join(page2_lines) + "\n"
stream1_bytes = unicodedata.normalize("NFKD", stream1).encode("ascii", "ignore")
stream2_bytes = unicodedata.normalize("NFKD", stream2).encode("ascii", "ignore")

objects = []
objects.append(b"1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n")
objects.append(b"2 0 obj\n<< /Type /Pages /Kids [3 0 R 6 0 R] /Count 2 >>\nendobj\n")
objects.append(b"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n")
objects.append(b"4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n")
objects.append(b"5 0 obj\n<< /Length %d >>\nstream\n" % len(stream1_bytes) + stream1_bytes + b"endstream\nendobj\n")
objects.append(b"6 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 7 0 R >>\nendobj\n")
objects.append(b"7 0 obj\n<< /Length %d >>\nstream\n" % len(stream2_bytes) + stream2_bytes + b"endstream\nendobj\n")

pdf = b"%PDF-1.4\n%\xe2\xe3\xcf\xd3\n"
xref_positions = []
for obj in objects:
    xref_positions.append(len(pdf))
    pdf += obj

xref_start = len(pdf)
pdf += b"xref\n0 %d\n0000000000 65535 f \n" % (len(objects) + 1)
for pos in xref_positions:
    pdf += f"{pos:010d} 00000 n \n".encode("ascii")
pdf += b"trailer\n<< /Size %d /Root 1 0 R >>\nstartxref\n%d\n%%EOF\n" % (len(objects) + 1, xref_start)

output_path = Path(__file__).resolve().parent / "src" / "TekinTeknikServis.Web" / "wwwroot" / "reports" / "degisiklik-raporu-9hafta-final.pdf"
output_path.write_bytes(pdf)
print(f"Created {output_path}")
