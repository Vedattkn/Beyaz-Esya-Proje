using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TekinTeknikServis.Core.Filters;
using TekinTeknikServis.Core.Models;
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

        public async Task<IActionResult> ServiceRequests(string? q = null, long? selectedId = null)
        {
            var reqs = await _supabase.GetAllServiceRequestsAsync();
            var filtered = reqs
                .Where(x =>
                    string.IsNullOrWhiteSpace(q) ||
                    (x.AdSoyad?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Telefon?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    x.Id?.ToString().Contains(q, StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(x => x.AdSoyad)
                .ThenByDescending(x => x.KayitTarihi ?? DateTime.MinValue)
                .ToList();

            var selected = selectedId.HasValue
                ? filtered.FirstOrDefault(x => x.Id == selectedId.Value)
                : filtered.FirstOrDefault();

            var model = new AdminServiceRequestsViewModel
            {
                Query = q ?? string.Empty,
                Requests = filtered,
                SelectedRequest = selected
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateServiceRequest(long id, string? adminCevabi, string? q)
        {
            try
            {
                var normalizedMessage = (adminCevabi ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(normalizedMessage))
                {
                    TempData["AdminError"] = "Mesaj boş olamaz.";
                    return RedirectToAction("ServiceRequests", new { q, selectedId = id });
                }

                await _supabase.UpdateAdminReplyAsync(id, normalizedMessage, "Inceleniyor");
                TempData["AdminSuccess"] = "Admin cevabı kaydedildi.";
            }
            catch (System.Exception ex)
            {
                // Hata olduğunda talepler listesine hata mesajı ile dön
                TempData["AdminError"] = "Talebi güncellerken hata oluştu: " + ex.Message;
            }
            return RedirectToAction("ServiceRequests", new { q, selectedId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceServiceRequestStatus(long id, string? q)
        {
            try
            {
                await _supabase.AdvanceServiceRequestStatusAsync(id);
                TempData["AdminSuccess"] = "Talep durumu güncellendi.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Talep durumu güncellenirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("ServiceRequests", new { q, selectedId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServiceRequest(long id, string? q)
        {
            try
            {
                await _supabase.DeleteServisTalebiAsync(id);
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Sohbet silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("ServiceRequests", new { q });
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
            if (string.IsNullOrWhiteSpace(product.Category)) ModelState.AddModelError("Category", "Kategori boş olamaz.");

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
            var categoriesFromProducts = products
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => x.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var categoriesFromTable = await _supabase.GetProductCategoriesAsync();
            ViewBag.Categories = categoriesFromTable
                .Concat(categoriesFromProducts)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCategoryToSelected(System.Collections.Generic.List<string> selectedIds, string? existingCategory, string? newCategoryName)
        {
            var category = !string.IsNullOrWhiteSpace(newCategoryName)
                ? newCategoryName.Trim()
                : (existingCategory ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(category))
            {
                TempData["AdminError"] = "Lütfen bir kategori seçin veya yeni kategori adı girin.";
                return RedirectToAction("Products");
            }

            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AdminError"] = "Kategori atamak için en az bir ürün seçin.";
                return RedirectToAction("Products");
            }

            try
            {
                await _supabase.AssignCategoryToProductsAsync(selectedIds, category);
                TempData["AdminSuccess"] = $"{selectedIds.Count} ürün '{category}' kategorisine atandı.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Kategori ataması sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string? newCategoryName)
        {
            var normalizedCategory = (newCategoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCategory))
            {
                TempData["AdminError"] = "Lütfen kategori adı girin.";
                return RedirectToAction("Products");
            }

            try
            {
                var created = await _supabase.CreateCategoryIfNotExistsAsync(normalizedCategory);
                TempData["AdminSuccess"] = created
                    ? $"'{normalizedCategory}' kategorisi eklendi."
                    : $"'{normalizedCategory}' kategorisi zaten mevcut.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Kategori eklenirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameCategory(string? oldCategoryName, string? renamedCategoryName)
        {
            var oldName = (oldCategoryName ?? string.Empty).Trim();
            var newName = (renamedCategoryName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                TempData["AdminError"] = "Eski ve yeni kategori adını girin.";
                return RedirectToAction("Products");
            }

            try
            {
                await _supabase.RenameCategoryAsync(oldName, newName);
                TempData["AdminSuccess"] = $"'{oldName}' kategorisi '{newName}' olarak güncellendi.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Kategori adı güncellenirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(string? categoryName)
        {
            var normalized = (categoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                TempData["AdminError"] = "Silinecek kategori adı boş olamaz.";
                return RedirectToAction("Products");
            }

            try
            {
                await _supabase.DeleteCategoryAsync(normalized);
                TempData["AdminSuccess"] = $"'{normalized}' kategorisi silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Kategori silinirken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveProductToCategory(string? productId, string? category)
        {
            var normalizedProductId = (productId ?? string.Empty).Trim();
            var normalizedCategory = (category ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedProductId) || string.IsNullOrWhiteSpace(normalizedCategory))
            {
                TempData["AdminError"] = "Ürün veya kategori bilgisi eksik.";
                return RedirectToAction("Products");
            }

            try
            {
                await _supabase.UpdateProductCategoryAsync(normalizedProductId, normalizedCategory);
                TempData["AdminSuccess"] = $"Ürün '{normalizedCategory}' kategorisine taşındı.";
            }
            catch (System.Exception ex)
            {
                TempData["AdminError"] = "Ürün kategoriye taşınırken hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Products");
        }

        public class MoveCategoryProductsRequest
        {
            public string? SourceCategory { get; set; }
            public string? TargetCategory { get; set; }
        }

        public class RemoveProductFromCategoryRequest
        {
            public string? ProductId { get; set; }
            public string? SourceCategory { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveCategoryProductsAjax([FromBody] MoveCategoryProductsRequest request)
        {
            var source = (request?.SourceCategory ?? string.Empty).Trim();
            var target = (request?.TargetCategory ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
                return BadRequest(new { success = false, message = "Kaynak ve hedef kategori zorunludur." });

            if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Kaynak ve hedef kategori aynı olamaz." });

            try
            {
                var products = await _supabase.GetAllProductsAsync();
                var idsToMove = products
                    .Where(p => !string.IsNullOrWhiteSpace(p.Id) && string.Equals((p.Category ?? string.Empty).Trim(), source, StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!idsToMove.Any())
                {
                    return Ok(new { success = true, movedCount = 0, message = "Taşınacak ürün bulunamadı." });
                }

                await _supabase.AssignCategoryToProductsAsync(idsToMove, target);
                return Ok(new { success = true, movedCount = idsToMove.Count, message = $"{idsToMove.Count} ürün '{source}' kategorisinden '{target}' kategorisine taşındı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kategori taşıma sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductFromCategoryAjax([FromBody] RemoveProductFromCategoryRequest request)
        {
            var productId = (request?.ProductId ?? string.Empty).Trim();
            var sourceCategory = (request?.SourceCategory ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(productId))
                return BadRequest(new { success = false, message = "Ürün bilgisi eksik." });

            if (string.IsNullOrWhiteSpace(sourceCategory))
                return BadRequest(new { success = false, message = "Bu ürün zaten kategorisiz." });

            try
            {
                await _supabase.ClearProductCategoryAsync(productId);
                return Ok(new { success = true, message = "Ürün kategoriden çıkarıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Ürün kategoriden çıkarılamadı: " + ex.Message });
            }
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
            if (string.IsNullOrWhiteSpace(product.Category)) ModelState.AddModelError("Category", "Kategori boş olamaz.");
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
