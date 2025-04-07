using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItaliaPizzaClient.Model
{
    public class OrderItem
    {
        public string SupplyName { get; set; }
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; }

        public string QuantityWithUnit => $"{Quantity} {MeasureUnit}";
    }

}