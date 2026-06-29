using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BuiDinhHieu_2380600640.Data;
using BuiDinhHieu_2380600640.Models.Domain;
using BuiDinhHieu_2380600640.Models.DTOs;
using BuiDinhHieu_2380600640.Services.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuiDinhHieu_2380600640.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateOrderAsync(CreateOrderRequest req)
        {
            // 1. Tạo entity Đơn hàng chính
            var order = new Order
            {
                OrderCode = "DH-" + DateTime.UtcNow.ToString("yyMMdd") + "-" + new Random().Next(1000, 9999),
                UserId = req.UserId,
                CustomerName = req.CustomerName,
                CustomerEmail = req.CustomerEmail,
                CustomerPhone = req.CustomerPhone,
                ShipAddress = req.ShipAddress,
                ShipCity = req.ShipCity,
                ShipCountry = req.ShipCountry ?? "Vietnam",
                Note = req.Note,
                PaymentMethod = req.PaymentMethod,
                PaymentStatus = "Unpaid",
                Status = "Pending",
                SubTotal = req.SubTotal,
                ShippingFee = req.ShippingFee,
                DiscountAmount = req.DiscountAmount,
                TotalAmount = req.TotalAmount,

                // CouponCode = req.CouponCode, <-- XÓA HOẶC COMMENT DÒNG NÀY (Vì DB không có cột này)
                OrderedAt = DateTime.UtcNow     // <-- SỬA CreatedAt THÀNH OrderedAt CHO KHỚP DB
            };

            // 2. Chuyển đổi List CartItems thành OrderDetails
            if (req.CartItems != null && req.CartItems.Any())
            {
                var orderDetails = new List<OrderDetail>();

                foreach (var item in req.CartItems)
                {
                    // Lấy thông tin sản phẩm từ DB để lấy ProductName
                    var product = await _context.Products.FindAsync(item.ProductId);

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        ProductName = product?.Name ?? "Sản phẩm không xác định", // <--- Lấy tên từ bảng Product
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.Quantity * item.UnitPrice,
                        SelectedColor = item.SelectedColor,
                        SelectedSize = item.SelectedSize
                    });
                }
                order.OrderDetails = orderDetails;
            }

            // 3. Lưu xuống Database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order.Id;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateStatusAsync(int id, string newStatus, string adminId)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) throw new Exception("Order not found");

            // Simplified State Machine validation
            string currentStatus = order.Status ?? "Pending";

            bool validTransition = false;
            switch (currentStatus)
            {
                case "Pending":
                    validTransition = (newStatus == "Confirmed" || newStatus == "Cancelled");
                    break;
                case "Confirmed":
                    validTransition = (newStatus == "Processing" || newStatus == "Cancelled");
                    break;
                case "Processing":
                    validTransition = (newStatus == "Shipped");
                    break;
                case "Shipped":
                    validTransition = (newStatus == "Delivered");
                    break;
                case "Delivered":
                    validTransition = (newStatus == "Refunded");
                    break;
            }

            if (!validTransition)
            {
                throw new Exception($"Cannot transition order from {currentStatus} to {newStatus}");
            }

            order.Status = newStatus;
            order.ProcessedByAdminId = adminId;
            
            // Set timestamps based on status
            var now = DateTime.UtcNow;
            if (newStatus == "Confirmed") order.ConfirmedAt = now;
            if (newStatus == "Shipped") order.ShippedAt = now;
            if (newStatus == "Delivered") order.DeliveredAt = now;
            
            if (newStatus == "Cancelled")
            {
                order.CancelledAt = now;
                // Return stock
                var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == id).ToListAsync();
                foreach (var detail in orderDetails)
                {
                    if (detail.ProductId.HasValue)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId.Value);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(int id, string userId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) throw new Exception("Order not found");
            
            if (order.UserId != userId) throw new Exception("Unauthorized to cancel this order");
            
            if (order.Status != "Pending") throw new Exception("Only Pending orders can be cancelled");

            order.Status = "Cancelled";
            order.CancelledAt = DateTime.UtcNow;

            foreach (var detail in order.OrderDetails)
            {
                if (detail.ProductId.HasValue)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId.Value);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // ── Methods mới cho VNPay ────────────────────────────────────────────

        /// <summary>
        /// Tìm đơn hàng theo OrderCode (VD-XXXXXX) — VNPay trả về trong vnp_TxnRef.
        /// </summary>
        public async Task<Order?> GetByOrderCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode ||
                                          o.Id.ToString() == orderCode);
        }

        /// <summary>
        /// Sau khi VNPay xác nhận thành công, cập nhật đơn hàng:
        /// - PaymentStatus: "Unpaid" → "Paid"
        /// - Status: "Pending" → "Confirmed"
        /// </summary>
        public async Task MarkAsPaidAsync(string orderCode, string transactionId, string paymentProvider)
        {
            var order = await GetByOrderCodeAsync(orderCode);
            if (order == null) return;

            // Chỉ cập nhật nếu chưa thanh toán (tránh cập nhật trùng lặp)
            if (order.PaymentStatus != "Paid")
            {
                order.PaymentStatus = "Paid";
                order.Status = "Confirmed";
                order.ConfirmedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private string BuildCartXml(List<CartItemDto> items)
        {
            var doc = new XDocument(new XElement("cart"));
            foreach (var item in items)
            {
                var el = new XElement("item",
                    new XAttribute("productId", item.ProductId),
                    new XAttribute("qty", item.Quantity)
                );
                if (!string.IsNullOrEmpty(item.SelectedColor))
                {
                    el.Add(new XAttribute("color", item.SelectedColor));
                }
                if (!string.IsNullOrEmpty(item.SelectedSize))
                {
                    el.Add(new XAttribute("size", item.SelectedSize));
                }
                doc.Root?.Add(el);
            }
            return doc.ToString();
        }
    }
}
