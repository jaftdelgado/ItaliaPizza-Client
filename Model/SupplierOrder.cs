using System.Collections.Generic;
using System;
using MeasureUnitModel = ItaliaPizzaClient.Model.MeasureUnit;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class SupplierOrder
    {
        public string OrderFolio { get; set; }

        public string SupplierName { get; set; }

        public int SupplierID { get; set; }

        public string OrderedDateFormatted { get; set; }

        public DateTime OrderedDate { get; set; }

        public decimal Total { get; set; }

        public DateTime? Delivered { get; set; }

        public int? Status { get; set; }

        public List<OrderedSupply> Items { get; set; }

        public string StatusDescription
        {
            get
            {
                var resources = Application.Current.Resources;

                switch (Status)
                {
                    case 0:
                        return resources["OrdSupplier_StatusPending"] as string;
                    case 1:
                        return resources["OrdSupplier_StatusDelivered"] as string;
                    case 2:
                        return resources["OrdSupplier_StatusCancelled"] as string;
                    default:
                        return "Desconocido";
                }
            }
        }

    }


    public class OrderedSupply
    {
        public Supply Supply { get; set; }

        public decimal Quantity { get; set; }

        public decimal TotalPrice => Quantity * Supply.Price;

        public string FormattedTotal => $"{TotalPrice:C}";

        public string SupplyName => Supply?.SupplyName;

        public byte[] SupplyPic => Supply?.SupplyPic;

        public decimal UnitPrice => Supply?.Price ?? 0;

        public string Unit
        {
            get
            {
                var measureUnit = MeasureUnitModel.GetDefaultMeasureUnits()
                    .Find(mu => mu.Id == Supply?.MeasureUnit);

                return measureUnit?.Abbreviation ?? "u";
            }
        }
    }
}
