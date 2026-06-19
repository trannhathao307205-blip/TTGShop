using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTGShop.Models
{
    [Table("Orders")] // Đặt tên bảng rõ ràng trong SQL Server
    public class Order
    {
        [Key] // Xác định là Khóa chính
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận hàng")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam (ví dụ: 0912345678)")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng cụ thể")]
        [StringLength(500, ErrorMessage = "Địa chỉ quá dài, vui lòng rút gọn dưới 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Định nghĩa kiểu dữ liệu tiền tệ chính xác trong DB
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // "COD", "VNPAY"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Completed", "Cancelled"

        // Mối quan hệ 1 - Nhiều với OrderDetail
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}