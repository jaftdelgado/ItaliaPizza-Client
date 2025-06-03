using System;
using System.Collections.Generic;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class Order
    {
        public int OrderID { get; set; }

        public int? CustomerID { get; set; }

        public DateTime? OrderDate { get; set; }

        public string OrderDateFormatted => OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "";

        public decimal? Total { get; set; }

        public string FormattedTotal => $"{Total:C}";

        public bool? IsDelivery { get; set; }

        public int? PersonalID { get; set; }

        public string AttendedByName { get; set; }

        public int? DeliveryID { get; set; }

        public int Status { get; set; }

        public string TableNumber { get; set; }

        public List<OrderedProduct> Items { get; set; }

        public Delivery DeliveryInfo { get; set; }

        public string StatusName
        {
            get
            {
                var status = OrderStatus.GetDefaultOrderStatuses().Find(s => s.Id == Status);
                return status?.Name ?? "Desconocido";
            }
        }

        public string OrderClientInfo
        {
            get
            {
                if (IsDelivery == true)
                    return DeliveryInfo?.CustomerFullName ?? "Cliente no especificado";
                else
                    return $"Mesa: {TableNumber ?? "N/A"}";
            }
        }
    }

    public class OrderedProduct
    {
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public string Name { get; set; }

        public decimal? Price { get; set; }

        public byte[] ProductPic { get; set; }

        public decimal TotalPrice => (Price ?? 0) * Quantity;

        public string FormattedTotal => $"{TotalPrice:C}";

        public Product Product { get; set; }
    }

    public class Delivery
    {
        public int DeliveryID { get; set; }

        public int AddressID { get; set; }

        public int DeliveryDriverID { get; set; }

        public string CustomerFullName { get; set; }

        public string CustomerAddress { get; set; }

        public string DeliveryDriverName { get; set; }
    }
}
