using System.ComponentModel.DataAnnotations;

namespace TTGShop.Models
{
    public class CartItem
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(250)]
        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng đặt mua phải từ 1 đến 100 sản phẩm")]
        public int Quantity { get; set; }

        public decimal TotalPrice => Price * Quantity;
    }
}