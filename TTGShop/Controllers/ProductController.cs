using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TTGShop.Models;
using TTGShop.Repositories;

namespace TTGShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // 1. CẬP NHẬT ACTION INDEX: Hỗ trợ tìm kiếm theo từ khóa và lọc danh mục
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            // Luôn lấy danh sách danh mục để đổ dữ liệu cho menu dropdown ở Layout
            var categoriesList = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categoriesList;

            // Lấy toàn bộ sản phẩm từ cơ sở dữ liệu
            var products = await _productRepository.GetAllAsync();

            // Lọc sản phẩm theo Danh mục nếu có yêu cầu từ Menu điều hướng
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            // Lọc sản phẩm theo Từ khóa tìm kiếm (Tìm kiếm gần đúng)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();

                // Tìm kiếm theo: Tên sản phẩm, hoặc Mô tả sản phẩm, hoặc tên Danh mục của sản phẩm đó
                products = products.Where(p =>
                    p.Name.ToLower().Contains(searchString) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchString)) ||
                    (categoriesList.FirstOrDefault(c => c.Id == p.CategoryId)?.Name.ToLower().Contains(searchString) ?? false)
                ).ToList();
            }

            return View(products);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();

            // Giữ lại SelectList phục vụ riêng cho thẻ <select> chọn danh mục tại View Add
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

            // Nếu lỗi ModelState, nạp lại dữ liệu tránh lỗi Layout
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Đồng bộ dữ liệu menu Layout cho trang chi tiết sản phẩm
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(product);
        }

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

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Đồng bộ dữ liệu menu Layout cho trang xác nhận xóa sản phẩm
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

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