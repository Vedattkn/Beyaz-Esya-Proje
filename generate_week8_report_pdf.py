from pathlib import Path

lines = [
    "8. Hafta Calisma Raporu",
    "",
    "Proje: Tekin Teknik Servis",
    "Tarih: 30.04.2026",
    "",
    "Genel Ozet:",
    "- Sepet ve siparis sistemi tamamlandi, veriler DB'ye yazildi.",
    "- Ana sayfa guven bar'i, marka gruplari ve modern kartlarla guncellendi.",
    "- Mustreri yorum bolumu tasarim olarak eklendi ve statik gosterime cekildi.",
    "",
    "Yapilan Isler:",
    "- Sepet/siparis akis duzenlemeleri ve checkout iyilestirmeleri.",
    "- Ana sayfa: guven, markalar, yorumlar bolumleri.",
    "- UI tarafinda kart, grid ve hover etkileri.",
    "",
    "Teknik Detaylar:",
    "- EF Core ile siparis ve siparis kalemleri tablolarinin kullanimi.",
    "- Razor view ve CSS tarafinda modern yerlesim ve tipografi.",
    "",
    "Test ve Dogrulama:",
    "- Sepete urun ekleme ve siparis tamamlama akisi manuel test edildi.",
    "- Ana sayfa bolumleri masaustu/mobil gorunumde kontrol edildi.",
    "",
    "Sonraki Adimlar:",
    "- Siparis takip ve durum guncelleme ekranlari eklenebilir.",
    "- Erisilebilirlik ve performans iyilestirmeleri.",
]

content_lines = []
content_lines.append("BT")
content_lines.append("/F1 18 Tf")
content_lines.append("50 770 Td (" + lines[0].replace("(", "\\(").replace(")", "\\)") + ") Tj")
content_lines.append("ET")
content_lines.append("BT")
content_lines.append("/F1 12 Tf")
content_lines.append("50 750 Td (" + lines[1].replace("(", "\\(").replace(")", "\\)") + ") Tj")
content_lines.append("ET")

y = 730
for line in lines[2:]:
    safe = line.replace("(", "\\(").replace(")", "\\)").replace("\\", "\\\\")
    content_lines.append("BT")
    content_lines.append("/F1 12 Tf")
    content_lines.append(f"50 {y} Td ({safe}) Tj")
    content_lines.append("ET")
    y -= 16

stream_text = "\n".join(content_lines) + "\n"
stream_bytes = stream_text.encode("latin1")

objects = []
objects.append(b"1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n")
objects.append(b"2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n")
objects.append(b"3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n")
objects.append(b"4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n")
objects.append(b"5 0 obj\n<< /Length %d >>\nstream\n" % len(stream_bytes) + stream_bytes + b"endstream\nendobj\n")

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

Path("8_Hafta_Raporu.pdf").write_bytes(pdf)
print("Created 8_Hafta_Raporu.pdf")
