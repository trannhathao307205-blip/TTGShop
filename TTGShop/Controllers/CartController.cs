using Microsoft.AspNetCore.Authorization; // >>> BẮT BUỘC PHẢI THÊM DÒNG NÀY ĐỂ DÙNG [Authorize]
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTGShop.Models;
using TTGShop.Repositories;
using TTGShop.Helpers;

namespace TTGShop.Controllers.Cart
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CartController(IProductRepository productRepository, IOrderRepository orderRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _categoryRepository = categoryRepository;
        }

        private List<CartItem> GetCart() => HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        private void SaveCart(List<CartItem> cart) => HttpContext.Session.SetJson("Cart", cart);

        // =========================================================================
        // CÁC TÍNH NĂNG GIỎ HÀNG: Cho phép cả khách vãng lai sử dụng thoải mái
        // =========================================================================

        // 1. TRANG GIỎ HÀNG
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(GetCart());
        }

        // 2. THÊM VÀO GIỎ
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // 3. XÓA KHỎI GIỎ
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // =========================================================================
        // CÁC TÍNH NĂNG THANH TOÁN: ÉP BUỘC PHẢI ĐĂNG NHẬP MỚI ĐƯỢC CHẠY
        // =========================================================================

        // 4. TRANG ĐẶT HÀNG (CHECKOUT)
        [Authorize] // >>> Chỉ cần đăng nhập (User hoặc Admin) là được vào mua
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = cart.Sum(x => x.TotalPrice);
            return View();
        }

        // 5. XỬ LÝ LƯU ĐƠN HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // >>> Chốt chặn bảo mật bắt login để tránh spam đơn nặc danh
        public async Task<IActionResult> ProcessCheckout(string customerName, string phone, string address, string email, string paymentMethod)
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            var order = new Order
            {
                CustomerName = customerName,
                Phone = phone,
                Address = address,
                Email = email,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(x => x.TotalPrice),
                Status = "Pending"
            };

            foreach (var item in cart)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            await _orderRepository.CreateOrderAsync(order);

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("OrderSuccess");
        }

        // 6. TRANG HOÀN THÀNH
        [Authorize] // >>> Chỉ hiển thị lời cảm ơn khi đúng tài khoản đó vừa đặt xong
        public async Task<IActionResult> OrderSuccess()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View();
        }
    }
}