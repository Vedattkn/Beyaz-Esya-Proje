from pathlib import Path
import unicodedata

lines = [
    '6. Hafta Final Raporu (Detayli)',
    '',
    'Kapsam: Sohbetin basindan sonuna kadar tum teknik adimlar, yapilan degisiklikler, karsilasilan hatalar, cozumler, yeni is kurallari ve plan disi kazanımlar.',
    'Tarih: 06.04.2026',
    '',
    'KPI:',
    '- 100% 6. hafta ana plan kalemleri tamamlandi',
    '- 20+ teknik iyilestirme ve hata duzeltmesi',
    '- DB-only mimariye kritik akislarda gecis',
    '- 2 buyuk yonetim akisi yeniden tasarlandi',
    '',
    'YoneticI Ozeti:',
    '- Baslangicta hedeflenen cart/checkout ve odeme akis duzeltmeleri tamamlandi.',
    '- Ardindan servis talep yonetimi, admin/musteri sohbet akislarinin tamamI yeniden ele alindi.',
    '- Urun/kategori yonetimi hem gorunum hem operasyonel acidan gelistirildi.',
    '- Supabase REST kaynakli auth/RLS sorunlari nedeniyle kritik akislar DB-only mimariye tasindi.',
    '- Kategori fallback mantIgi kaldIrIldI, kategori davranislari net is kurallariyla sabitlendi.',
    '',
    'Plana Gore Tamamlananlar:',
    '- Kullanici auth guclendirmeleri (BCrypt, JWT/session iyilestirmeleri, rol okuma).',
    '- Checkout/cart UX ve dogrulama duzeltmeleri.',
    '- Servis talep olusturma, listeleme, cevaplasma, durum akisi.',
    '- Admin panelinde urun ve kategori yonetimi.',
    '- EF migration altyapisi ve Supabase DB migration uygulamasi.',
    '',
    'Ekstra Kazanimlar:',
    '- Admin urun ekrani iki panelli, surukle-birak yonetime donustu.',
    '- AJAX modal ile kategori bazli toplu urun tasima eklendi.',
    '- Kategori etiketlerinden tek urun cikar (x) aksiyonu eklendi.',
    '- Kapanmis talep kilidi ve musteri cevap cooldown kurali eklendi.',
    '- Port cakismasi kalici olarak yeni launch portlariyla cozuldu.',
    '',
    'Kronolojik Akis:',
    '- Sepet ve odeme ekranlari iyilestirme talebi ile basladi.',
    '- Sepete urun eklendiginde adet gosteren badge eklendi; quantity render hatasi giderildi.',
    '- Admin servis talepleri ekrani sohbet odakli yapilandirildi; durum/yanit akisinda sadele?tirme yapildi.',
    '- Musteri tarafinda taleplerim ekraninda cevaplasma ve durum takibi guclendirildi.',
    '- Admin sohbet silme talebi eklendi; sonra yetki/REST kaynakli sorunlar gozlemlendi.',
    '',
    'Modul Bazli Detay:',
    '- Checkout/Cart: model validasyonlari, sepet adedi badge, quantity gorunum duzeltmeleri, UI iyilestirmeleri.',
    '- ServiceRequest (Musteri): kategori/parca form modeli, cevaplasma, kapatma, hata geri bildirimleri.',
    '- ServiceRequest (Admin): sohbet paneli, tek tusla durum ilerletme, sohbet silme, kapali talep kilidi.',
    '- Urun/Kategori Yonetimi: kategori ekle, isim guncelle, silme kurali, urun tasima, kategori ici urun cikar, drag-drop.',
    '- Data Access: REST fallbacklerinin kaldirilmasi, Npgsql DB-only servis metotlari.',
    '- Migration/Schema: EF migration altyapisi, kategoriler tablosu migration, schema uyum mekanizmasi.',
    '',
    'Hata Enventeri:',
    '- SecilenParcaId required: opsiyonel alan nullable degildi -> model nullable yapildi.',
    '- Talep kayitlari gorunmuyor: REST/RLS ve sessiz hata yutma -> DB-only insert/list ve acik hata gosterimi.',
    '- Admin talep update yetki hatasi: REST tutarsizligi -> Get/UpdateConversation DB-only.',
    '- Invalid API key: REST anahtar/policy bagimliligi -> kritik akislarda SupabaseDb kullanildi.',
    '- Port 5195 cakismasi: calisan process -> launch profile portlari degistirildi.',
    '- DLL lock: calisan web process build ciktilarini kilitledi -> process sonlandirip build tekrarlandi.',
    '- category kolonu yok: schema farki -> dinamik kolon cozumu + alter table self-heal.',
    '- Ismi guncelle yerine toplu silme tetiklenmesi: UI form cakismasi -> ayrik form/JS submit mimarisi.',
    '- Urunlu kategori silme: eski fallback mantigi -> urun varsa kategori silinmez kuralI.',
    '',
    'Yeni Is Kurallari:',
    '- Kapali talepte admin ve musteri yeni mesaj gonderemez.',
    '- Musteri ayni talepte cevaplar arasinda 1 saat bekler.',
    '- Durum ilerletme admin tarafinda tek butonlu akisla yonetilir.',
    '- Icinde urun olan kategori silinemez.',
    '- Genel kategori fallback devre disidir.',
    '- Kategoriden cikarilan urun kategorisiz olur.',
    '',
    'Mimari Sonuc:',
    '- SupabaseService kritik islevlerde DB-only calisacak sekilde sertlestirildi.',
    '- Controller seviyesinde hata yutma azaltildi, anlamli geri bildirim veriliyor.',
    '- Admin urun/kategori ekrani daha sade, hizli ve operasyonel olarak olgunlasmisti.',
    '- Schema degisikliklerine dayanikli servis katmani olusturuldu.',
    '',
    'Build/Operasyon Notlari:',
    '- Bazi asamalarda acik process nedeniyle dosya kilidi olustu, process kapatilip tekrar build alinmistir.',
    '- 5195 port cakismasi gozlendi, launch profile portlari degistirilerek cozum uygulandi.',
    '',
    'Genel Sonuc:',
    '- 6. hafta plani tamamlaniyor ve sistemin servis talep yonetimi, kategori/urun operasyonlari ile veri erisim katmani daha dayaniklikli hale geliyor.',
]


def escape_pdf(text):
    return text.replace('(', '\\(').replace(')', '\\)').replace('\\', '\\\\')

content_lines = []
line_height = 12
font_size = 10
x = 50
page_height = 842

y = 820
content_lines.append('BT')
content_lines.append(f'/F1 {14} Tf')
content_lines.append(f'{x} {y} Td ({escape_pdf(lines[0])}) Tj')
content_lines.append('ET')
y -= 22

content_lines.append('BT')
content_lines.append(f'/F1 {10} Tf')
for idx, line in enumerate(lines[1:], start=1):
    if y < 40:
        break
    content_lines.append(f'{x} {y} Td ({escape_pdf(line)}) Tj')
    content_lines.append('ET')
    y -= 14
    if idx < len(lines):
        content_lines.append('BT')

page1_lines = content_lines.copy()
remaining_lines = lines[(idx+1):]

page2_lines = []
y = 820
page2_lines.append('BT')
page2_lines.append(f'/F1 {14} Tf')
page2_lines.append(f'{x} {y} Td ({escape_pdf("Devam eden 6. Hafta Detaylari")}) Tj')
page2_lines.append('ET')
y -= 22
page2_lines.append('BT')
page2_lines.append(f'/F1 {10} Tf')
for line in remaining_lines:
    if y < 40:
        break
    page2_lines.append(f'{x} {y} Td ({escape_pdf(line)}) Tj')
    page2_lines.append('ET')
    y -= 14
    if line != remaining_lines[-1]:
        page2_lines.append('BT')

stream1 = '\n'.join(page1_lines) + '\n'
stream2 = '\n'.join(page2_lines) + '\n'
stream1_bytes = unicodedata.normalize('NFKD', stream1).encode('ascii', 'ignore')
stream2_bytes = unicodedata.normalize('NFKD', stream2).encode('ascii', 'ignore')

objects = []
objects.append(b'1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n')
objects.append(b'2 0 obj\n<< /Type /Pages /Kids [3 0 R 6 0 R] /Count 2 >>\nendobj\n')
objects.append(b'3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n')
objects.append(b'4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n')
objects.append(b'5 0 obj\n<< /Length %d >>\nstream\n' % len(stream1_bytes) + stream1_bytes + b'endstream\nendobj\n')
objects.append(b'6 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 7 0 R >>\nendobj\n')
objects.append(b'7 0 obj\n<< /Length %d >>\nstream\n' % len(stream2_bytes) + stream2_bytes + b'endstream\nendobj\n')

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

Path('6_Hafta_Raporu_Yeni.pdf').write_bytes(pdf)
print('Created 6_Hafta_Raporu_Yeni.pdf')
