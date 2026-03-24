# Node.js (Express) → ASP.NET MVC 5 (.NET Framework) geçiş rehberi

Bu repo şu an `server.js` içinde tek dosyada Express route’ları, `express-session` ile sepet ve Supabase’e form kaydı içeriyor. Bu doküman, aynı davranışı **ASP.NET MVC 5 (.NET Framework)** üzerinde **MVC katmanlarına bölerek** taşıma yolunu anlatır ve `dotnet-mvc5/src/` altında örnek bir iskelet verir.

## 1) Mevcut Node route’larının MVC5 karşılığı

Node (`server.js`) tarafındaki endpoint’ler:

- `GET /` → **HomeController.Index**
- `GET /hizmetler` → **HomeController.Hizmetler**
- `GET /urunler` → **HomeController.Urunler**
- `GET /iletisim` → **HomeController.Iletisim**
- `GET /servis-talep` → **ServiceRequestController.Index (GET)**
- `POST /servis-talep` → **ServiceRequestController.Index (POST)** (Supabase insert)

Sepet (session):

- `GET /sepet` → **CartController.Index**
- `POST /sepet/ekle/{id}` → **CartController.Add**
- `POST /sepet/sil/{index}` → **CartController.RemoveAt**
- `POST /sepet/guncelle/{index}` → **CartController.UpdateQuantity**

Ödeme:

- `GET /odeme` → **CheckoutController.Index (GET)**
- `POST /odeme/tamamla` → **CheckoutController.Complete (POST)** (demo/validasyon)

Ürün detayı:

- `GET /urun/{id}` → **ProductsController.Detail**

## 2) Node’daki “public” ve “views” nereye taşınır?

- Node `public/css/style.css` → MVC5’te **`Content/style.css`**
- Node `views/*.html` → MVC5’te **`Views/<Controller>/<View>.cshtml`**

Pratik yöntem:

- HTML’i `.cshtml` içine kopyala.
- `<link rel="stylesheet" href="/css/style.css">` yerine `@Url.Content("~/Content/style.css")` kullan.
- Navbar’daki linkleri `@Url.Action(...)` ile üret (hardcode da çalışır ama MVC mantığına aykırı).

## 3) Session sepeti MVC5’te nasıl yapılır?

Node’da: `req.session.sepet = []`

MVC5’te: `Session["Cart"]` içine `List<CartItem>` koyuyoruz. Örnek kodlar `dotnet-mvc5/src/.../Controllers/CartController.cs` içinde.

## 4) Supabase insert’i MVC5’e taşıma

Node’da `@supabase/supabase-js` ile `.from('servis_talepleri').insert(...)` var.

MVC5’te 2 seçenek:

- **(A) Supabase .NET SDK** (NuGet: `Supabase`) — çoğu zaman NetStandard hedeflediği için MVC5’te de çalışır.
- **(B) HTTP ile PostgREST’e direkt çağrı** — bağımlılık az, kontrol sizde.

Bu iskelet (B) yaklaşımıyla gider: `Services/SupabaseService.cs`.

## 5) Güvenlik notu (önemli)

`server.js` içinde Supabase key hardcode görünüyor. Bu key “publishable” olsa bile iyi pratik değildir:

- Node tarafında `.env` kullanıp `process.env.SUPABASE_URL`, `process.env.SUPABASE_KEY` yapın
- `.env` dosyasını git’e koymayın
- MVC5 tarafında `Web.config` `appSettings` veya environment variable kullanın

## 6) Bu klasörde ne var?

`dotnet-mvc5/src/` altında:

- **Models**: form ve sepet modelleri
- **Services**: Supabase insert servisi + ürün kataloğu
- **Controllers**: Node route’larıyla birebir action’lar

> Not: Bu iskelet Visual Studio “ASP.NET Web Application (.NET Framework) → MVC” template’i ile açılacak bir projeye dosyaları kopyalamanız için hazırlanmıştır. İstersen burada tam `.csproj` ve MVC5 template çıktısını da oluşturabilirim.

