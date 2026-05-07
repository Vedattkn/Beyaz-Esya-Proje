from pathlib import Path

lines = [
    '7. Hafta Calisma Raporu',
    '',
    'Proje: Tekin Teknik Servis',
    '',
    'Hedef: Web sitesinin iletisim deneyimini profesyonel ve mobil uyumlu hale getirmek.',
    '',
    '1. Ana Sayfa Iletisim Bloklari',
    '- "Bize Hemen Ulasin" bolumunde telefon numarasi gorunumu kaldirildi.',
    '- Arama ve WhatsApp islemleri ikon-only butonlari ile yeniden tasarlandi.',
    '- Kartin beyaz arka plani ve kalin border/golge stili kaldirildi.',
    '- Iconlar daha buyuk, daha net ve ortali hale getirildi.',
    '',
    '2. Sabit Floating Iletisim Butonu',
    '- Alt sagda sabit bir mini iletisim widgeti eklendi.',
    '- Buton WhatsApp yesil temasina gore guncellendi.',
    '- Ilk acilis icin belirgin pulse animasyonu eklendi.',
    '',
    '3. Mobil Uyum ve Layout Duzeltmeleri',
    '- Sayfa geneli icin yatay overflow problemi onlendi.',
    '- .home-contact-card responsive olarak wrap edecek sekilde ayarlandi.',
    '- Mobil kirmiza boyunca butonlarin ve metinlerin hizalanmasi guclendirildi.',
    '',
    '4. Iletisim Sayfasi Guncellemeleri',
    '- Iletisim sayfasina ikon-only arama ve WhatsApp butonlari eklendi.',
    '- "Acil Teknik Destek Lazim mi?" bolumunun altina ikon grubu tasarlandi.',
    '- Mavi aksiyon kartinin yuksekligi optimize edildi.',
    '',
    '5. Stiller ve Tutarlilik',
    '- Tumu https://fonts.googleapis.com/ font uyumlu bir stil yapisi kullanildi.',
    '- Butonlarin hover efektleri ve gölge derinlikleri profesyonelce ayarlandi.',
    '',
    'Degistirilen dosyalar:',
    '- src/TekinTeknikServis.Web/Views/Home/Index.cshtml',
    '- src/TekinTeknikServis.Web/Views/Home/Iletisim.cshtml',
    '- src/TekinTeknikServis.Web/Views/Shared/_Layout.cshtml',
    '- src/TekinTeknikServis.Web/wwwroot/css/style.css',
    '',
    'Toplam Degisiklik:',
    '- 4 ana dosya degisimi',
    '- 395 satir degisikligi: 395 ekleme, 2 silme',
    '',
    'Onerilen sonraki adimlar:',
    '- Kullanici testi ve mobil cihazlarda son kontrol.',
    '- Iletisim aksiyonlarinin izlenmesi ve geri bildirim toplanmasi.',
]

content_lines = []
content_lines.append('BT')
content_lines.append('/F1 18 Tf')
content_lines.append('50 770 Td (' + lines[0].replace('(', '\\(').replace(')', '\\)') + ') Tj')
content_lines.append('ET')
content_lines.append('BT')
content_lines.append('/F1 12 Tf')
content_lines.append('50 750 Td (' + lines[1].replace('(', '\\(').replace(')', '\\)') + ') Tj')
content_lines.append('ET')

y = 730
for line in lines[2:]:
    safe = line.replace('(', '\\(').replace(')', '\\)').replace('\\', '\\\\')
    content_lines.append('BT')
    content_lines.append('/F1 12 Tf')
    content_lines.append(f'50 {y} Td ({safe}) Tj')
    content_lines.append('ET')
    y -= 16

stream_text = '\n'.join(content_lines) + '\n'
stream_bytes = stream_text.encode('latin1')

objects = []
objects.append(b'1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n')
objects.append(b'2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n')
objects.append(b'3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n')
objects.append(b'4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n')
objects.append(b'5 0 obj\n<< /Length %d >>\nstream\n' % len(stream_bytes) + stream_bytes + b'endstream\nendobj\n')

pdf = b'%PDF-1.4\n%\xe2\xe3\xcf\xd3\n'
xref_positions = []
for obj in objects:
    xref_positions.append(len(pdf))
    pdf += obj

xref_start = len(pdf)
pdf += b'xref\n0 %d\n0000000000 65535 f \n' % (len(objects) + 1)
for pos in xref_positions:
    pdf += f'{pos:010d} 00000 n \n'.encode('ascii')
pdf += b'trailer\n<< /Size %d /Root 1 0 R >>\nstartxref\n%d\n%%EOF\n' % (len(objects) + 1, xref_start)

Path('7_Hafta_Raporu.pdf').write_bytes(pdf)
print('Created 7_Hafta_Raporu.pdf')
