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

        public string Status { get; set; }

        public bool? IsDelivery { get; set; }

        public int? PersonalID { get; set; }

        public int? IDDelivery { get; set; }

        public int? IDState { get; set; }

        public List<OrderedProduct> Items { get; set; }

        public string StatusDescription
        {
            get
            {
                var resources = Application.Current.Resources;

                if (Status == null) return "Desconocido";

                switch (Status.ToLower())
                {
                    case "pending":
                        return resources["Order_StatusPending"] as string;
                    case "completed":
                        return resources["Order_StatusCompleted"] as string;
                    case "cancelled":
                        return resources["Order_StatusCancelled"] as string;
                    default:
                        return "Desconocido";
                }
            }
        }
    }

    public class OrderedProduct
    {
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal? TotalPrice => (Product?.Price ?? 0) * Quantity;

        public string FormattedTotal => $"{TotalPrice:C}";

        public string ProductName => Product?.Name;

        public byte[] ProductPic => Product?.ProductPic;

        public decimal UnitPrice => Product?.Price ?? 0;

    }
}
