using TekinTeknikServis.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(1);
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHttpClient<SupabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(name: "home", pattern: "", defaults: new { controller = "Home", action = "Index" });
app.MapControllerRoute(name: "hizmetler", pattern: "hizmetler", defaults: new { controller = "Home", action = "Hizmetler" });
app.MapControllerRoute(name: "urunler", pattern: "urunler", defaults: new { controller = "Home", action = "Urunler" });
app.MapControllerRoute(name: "iletisim", pattern: "iletisim", defaults: new { controller = "Home", action = "Iletisim" });
app.MapControllerRoute(name: "servis-talep", pattern: "servis-talep", defaults: new { controller = "ServiceRequest", action = "Index" });
app.MapControllerRoute(name: "sepet", pattern: "sepet", defaults: new { controller = "Cart", action = "Index" });
app.MapControllerRoute(name: "odeme", pattern: "odeme", defaults: new { controller = "Checkout", action = "Index" });
app.MapControllerRoute(name: "urun-detay", pattern: "urun/{id}", defaults: new { controller = "Products", action = "Detail" });

// Kullanıcı Yönetimi Rotaları
app.MapControllerRoute(name: "giris", pattern: "giris", defaults: new { controller = "Account", action = "Login" });
app.MapControllerRoute(name: "kayit", pattern: "kayit", defaults: new { controller = "Account", action = "Register" });
app.MapControllerRoute(name: "profil", pattern: "profil", defaults: new { controller = "Account", action = "Profile" });
app.MapControllerRoute(name: "cikis", pattern: "cikis", defaults: new { controller = "Account", action = "Logout" });

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
