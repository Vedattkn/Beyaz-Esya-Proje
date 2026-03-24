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
builder.Services.AddHttpClient<SupabaseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "home", pattern: "", defaults: new { controller = "Home", action = "Index" }).WithStaticAssets();
app.MapControllerRoute(name: "hizmetler", pattern: "hizmetler", defaults: new { controller = "Home", action = "Hizmetler" }).WithStaticAssets();
app.MapControllerRoute(name: "urunler", pattern: "urunler", defaults: new { controller = "Home", action = "Urunler" }).WithStaticAssets();
app.MapControllerRoute(name: "iletisim", pattern: "iletisim", defaults: new { controller = "Home", action = "Iletisim" }).WithStaticAssets();
app.MapControllerRoute(name: "servis-talep", pattern: "servis-talep", defaults: new { controller = "ServiceRequest", action = "Index" }).WithStaticAssets();
app.MapControllerRoute(name: "sepet", pattern: "sepet", defaults: new { controller = "Cart", action = "Index" }).WithStaticAssets();
app.MapControllerRoute(name: "odeme", pattern: "odeme", defaults: new { controller = "Checkout", action = "Index" }).WithStaticAssets();
app.MapControllerRoute(name: "urun-detay", pattern: "urun/{id}", defaults: new { controller = "Products", action = "Detail" }).WithStaticAssets();

// Kullanıcı Yönetimi Rotaları
app.MapControllerRoute(name: "giris", pattern: "giris", defaults: new { controller = "Account", action = "Login" }).WithStaticAssets();
app.MapControllerRoute(name: "kayit", pattern: "kayit", defaults: new { controller = "Account", action = "Register" }).WithStaticAssets();
app.MapControllerRoute(name: "profil", pattern: "profil", defaults: new { controller = "Account", action = "Profile" }).WithStaticAssets();
app.MapControllerRoute(name: "cikis", pattern: "cikis", defaults: new { controller = "Account", action = "Logout" }).WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
