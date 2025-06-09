using System.Collections.Generic;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceKey { get; set; }

        public OrderStatus(int id, string nameKeyBase)
        {
            Id = id;
            ResourceKey = $"OrdStatus_{nameKeyBase}";
            Name = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<OrderStatus> GetDefaultOrderStatuses()
        {
            return new List<OrderStatus>
            {
                new OrderStatus(0, "Canceled"),
                new OrderStatus(1, "Taken"),
                new OrderStatus(2, "Preparing"),
                new OrderStatus(3, "Prepared"),
                new OrderStatus(4, "Sent"),
                new OrderStatus(5, "Delivered"),
                new OrderStatus(6, "Paid"),
                new OrderStatus(7, "NotDelivered")
            };
        }
    }
}
