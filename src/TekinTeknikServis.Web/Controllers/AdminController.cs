using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TekinTeknikServis.Core.Filters;
using TekinTeknikServis.Core.Services;

namespace TekinTeknikServis.Core.Controllers
{
    [AdminCheck]
    public class AdminController : Controller
    {
        private readonly SupabaseService _supabase;

        public AdminController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> ServiceRequests()
        {
            var reqs = await _supabase.GetAllServiceRequestsAsync();
            return View(reqs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServiceRequest(long id, string durum, string adminCevabi)
        {
            try
            {
                await _supabase.UpdateAdminReplyAsync(id, adminCevabi ?? "", durum ?? "Bekliyor");
            }
            catch (System.Exception ex)
            {
                // Hata olduğunda talepler listesine hata mesajı ile dön
                TempData["AdminError"] = "Talebi güncellerken hata oluştu: " + ex.Message;
            }
            return RedirectToAction("ServiceRequests");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View(new Product());
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, Microsoft.AspNetCore.Http.IFormFile? ImageFile, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(product.Name)) ModelState.AddModelError("Name", "Ürün adı boş olamaz.");

            if (!ModelState.IsValid || ImageFile == null) 
            {
                if (ImageFile == null) ModelState.AddModelError("", "Lütfen bir ürün resmi seçin.");
                return View(product);
            }
            
            try
            {
                // DEBUG: Model data check
                Console.WriteLine($"Ürün Ekleme: {product.Name}, Fiyat: {product.PriceText}, Resim: {ImageFile?.FileName}");

                // Resim Kaydetme İşlemleri
                if (ImageFile == null || string.IsNullOrWhiteSpace(ImageFile.FileName))
                    return BadRequest("Resim dosyası gereklidir.");

                var ext = System.IO.Path.GetExtension(ImageFile.FileName);
                var fileName = System.Guid.NewGuid().ToString("N") + ext;
                var uploadsFolder = System.IO.Path.Combine(env.WebRootPath, "images", "products");

                if (!System.IO.Directory.Exists(uploadsFolder))
                    System.IO.Directory.CreateDirectory(uploadsFolder);

                var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/products/" + fileName;

                // ID Üretimi (Slug)
                var generatedId = product.Name.ToLower().Replace(" ", "-")
                                    .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
                                    .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                
                // Basit temizlik
                generatedId = System.Text.RegularExpressions.Regex.Replace(generatedId, @"[^a-z0-9\-]", "");
                product.Id = generatedId + "-" + System.DateTime.Now.Ticks.ToString().Substring(12); // Benzersiz yapmak için

                await _supabase.InsertProductAsync(product);
                ViewBag.Success = true;
                return View(new Product());
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "Ürün eklenirken hata oluştu: " + ex.Message);
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _supabase.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string id)
        {
            var product = await _supabase.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, Microsoft.AspNetCore.Http.IFormFile? ImageFile, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(product.Name)) ModelState.AddModelError("Name", "Ürün adı boş olamaz.");
            if (!ModelState.IsValid) return View(product);

            try
            {
                // DEBUG: Model data check
                Console.WriteLine($"Ürün Düzenleme: {product.Id}, İsim: {product.Name}, Fiyat: {product.PriceText}, Yeni Resim: {ImageFile?.FileName}");

                if (ImageFile != null)
                {
                    var ext = System.IO.Path.GetExtension(ImageFile.FileName);
                    var fileName = System.Guid.NewGuid().ToString("N") + ext;
                    var uploadsFolder = System.IO.Path.Combine(env.WebRootPath, "images", "products");

                    if (!System.IO.Directory.Exists(uploadsFolder))
                        System.IO.Directory.CreateDirectory(uploadsFolder);

                    var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/images/products/" + fileName;
                }
                
                // If existing imageUrl was empty and no new file, we must at least supply existing
                if (string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    var existing = await _supabase.GetProductByIdAsync(product.Id);
                    if (existing != null) product.ImageUrl = existing.ImageUrl;
                }

                await _supabase.UpdateProductAsync(product);
                return RedirectToAction("Products");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "Güncelleme hatası: " + ex.Message);
                return View(product);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            try
            {
                await _supabase.DeleteProductAsync(id);
                TempData["AdminSuccess"] = "Ürün başarıyla silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Ürün silinirken hata oluştu: " + ex.Message;
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(System.Collections.Generic.List<string> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any()) 
            {
                TempData["AdminError"] = "Lütfen silinecek ürünleri seçin.";
                return RedirectToAction("Products");
            }

            try
            {
                await _supabase.DeleteProductsAsync(selectedIds);
                TempData["AdminSuccess"] = $"{selectedIds.Count} ürün başarıyla silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Toplu silme sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }
    }
}
