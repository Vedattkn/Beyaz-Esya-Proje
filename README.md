# 🛠️ Tekin Teknik Servis - Beyaz Eşya Yönetim Sistemi

Bu proje, modern bir beyaz eşya teknik servisi yönetim platformudur. Müşterilerin teknik servis taleplerini iletebildiği, ürünleri inceleyip satın alabildiği ve adminlerin tüm süreci yönetebildiği uçtan uca bir çözüm sunar.

![Tekin Teknik Servis](https://img.shields.io/badge/Status-Active-brightgreen)
![.NET Core](https://img.shields.io/badge/.NET-8.0-blue)
![Supabase](https://img.shields.io/badge/Database-Supabase-green)
![MVC](https://img.shields.io/badge/Architecture-MVC-red)

---

## 🌟 Öne Çıkan Özellikler

### 👤 Müşteri Portalı
- **Ürün Kataloğu:** Şık bir arayüzle beyaz eşyaların listelenmesi ve detaylandırılması.
- **Akıllı Sepet:** Ürün ekleme, miktar güncelleme ve gerçek zamanlı sepet yönetimi.
- **Hızlı Servis Talebi:** Arıza bildirimlerini kolayca oluşturabilme.
- **Talep Takibi:** Oluşturulan servis taleplerinin durumunu kullanıcı profilinden izleyebilme.
- **Güvenli Giriş:** Supabase tabanlı kullanıcı kimlik doğrulama sistemi.

### 🛡️ Admin Yönetim Paneli
- **Ürün Yönetimi:** Stoktaki ürünlerin eklenmesi, görsellerinin yüklenmesi ve fiyat güncellemeleri.
- **Toplu İşlemler:** Çoklu ürün silme ve gelişmiş filtreleme seçenekleri.
- **Talep Yönetimi:** Tüm müşteri servis taleplerini görüntüleme ve yönetme.
- **Dinamik İçerik:** Tüm verilerin Supabase üzerinden dinamik olarak yönetilmesi.

---

## 🏗️ Mimari ve Teknoloji Yığını

- **Framework:** ASP.NET Core MVC
- **Data & Auth:** [Supabase](https://supabase.io) (PostgreSQL + Auth + Storage)
- **Email:** Google SMTP Integration (Otomatik bilgilendirme sistemleri)
- **Frontend:** HTML5, CSS3, Vanilla JavaScript, FontAwesome
- **Validation:** Server-side ve Client-side doğrulamalar

---

## ⚙️ Kurulum ve Çalıştırma

### 1. Gereksinimler
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- Visual Studio 2022 veya VS Code
- Supabase Hesabı

### 2. Yapılandırma
`src/TekinTeknikServis.Core/appsettings.json` dosyasını kendi anahtarlarınızla güncelleyin:

```json
"Supabase": {
  "Url": "https://your-project.supabase.co",
  "Key": "your-anon-key"
},
"Email": {
  "SmtpUser": "your-email@gmail.com",
  "SmtpPassword": "your-app-password"
}
```

### 3. Çalıştırma
```bash
# Proje dizinine gidin
cd src/TekinTeknikServis.Core

# Bağımlılıkları geri yükleyin
dotnet restore

# Uygulamayı başlatın
dotnet run
```

---

## 📁 Proje Yapısı

```text
TekinTeknikServis.Core/
├── Controllers/       # İstek karşılayıcılar (Admin, Account, Product...)
├── Models/            # Veri modelleri ve ViewModel'lar
├── Views/             # Razor View arayüzleri
├── Services/          # Supabase, Email ve İş mantığı servisleri
├── Infrastructure/    # Custom filtreler ve middleware
└── wwwroot/           # CSS, JS ve statik dosyalar
```

---

## 🤝 Katkıda Bulunma

1. Bu depoyu çatallayın (Fork).
2. Yeni bir özellik dalı (Branch) oluşturun (`git checkout -b feature/yeniozellik`).
3. Değişikliklerinizi yapın ve kaydedin.
4. Dalınızı gönderin (Push) (`git push origin feature/yeniozellik`).
5. Bir Çekme İsteği (Pull Request) oluşturun.

---

**Geliştirici:** [Vedat Tekin](https://github.com/Vedattkn)
**Lisans:** MIT Lisansı altında lisanslanmıştır.
