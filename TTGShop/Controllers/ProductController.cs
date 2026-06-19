using Microsoft.AspNetCore.Authorization; // >>> BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ DÙNG [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TTGShop.Models;
using TTGShop.Repositories;

namespace TTGShop.Controllers
{
    // Cấp quyền cao nhất cho toàn bộ Controller: Mặc định chỉ có Admin mới được phép vào đây
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // =========================================================================
        // HÀM INDEX: Mở khóa cho tất cả mọi người (Kể cả khách chưa đăng nhập) xem hàng
        // =========================================================================
        [AllowAnonymous] // >>> THÊM DÒNG NÀY (Ai cũng xem được danh sách sản phẩm)
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            var categoriesList = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categoriesList;
            ViewData["CurrentFilter"] = searchString;

            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = await _productRepository.SearchProductsAsync(searchString);
            }
            else
            {
                products = await _productRepository.GetAllAsync();
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            return View(products);
        }

        // =========================================================================
        // HÀM DISPLAY: Mở khóa cho tất cả mọi người xem chi tiết cấu hình máy
        // =========================================================================
        [AllowAnonymous] // >>> THÊM DÒNG NÀY (Ai cũng xem được chi tiết sản phẩm)
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);
        }

        // =========================================================================
        // CÁC HÀM DƯỚI ĐÂY KHÔNG CÓ THẺ [AllowAnonymous] SẼ TỰ ĐỘNG THỪA HƯỞNG 
        // QUYỀN [Authorize(Roles = "Admin")] TỪ TRÊN ĐẦU CLASS (CHỈ ADMIN MỚI ĐƯỢC VÀO)
        // =========================================================================

        // 1. Thêm sản phẩm
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 2. Sửa sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null) return NotFound();

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                if (imageUrl != null)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 3. Xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Hàm phụ xử lý file ảnh
        private async Task<string> SaveImage(IFormFile image)
        {
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            var fileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(imagesFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return "/images/" + fileName;
        }
    }
}