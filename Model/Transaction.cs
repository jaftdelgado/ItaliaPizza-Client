using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ItaliaPizzaClient.Model
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int CashRegisterID { get; set; }
        public string FinancialFlow { get; set; }
        public decimal? Amount { get; set; }
        public DateTime Date { get; set; }
        public int Concept { get; set; }
        public string Description { get; set; }
        public int? OrderID { get; set; }
        public int? SupplierOrderID { get; set; }

        // Fecha formateada
        public string FormattedDate => Date.ToString("dd/MM/yyyy HH:mm");

        // Monto formateado con signo según el flujo
        public string FormattedAmountWithSign
        {
            get
            {
                if (!Amount.HasValue) return "$0.00";

                var absoluteAmount = Math.Abs(Amount.Value);
                var sign = FinancialFlow == "I" ? "+" : "-";
                return $"{sign}{absoluteAmount.ToString("C")}";
            }
        }

        // Versión alternativa con color (opcional)
        public string FormattedAmountColored
        {
            get
            {
                if (!Amount.HasValue) return "$0.00";

                var absoluteAmount = Math.Abs(Amount.Value);
                return FinancialFlow == "I"
                    ? $"+{absoluteAmount.ToString("C")}"
                    : $"-{absoluteAmount.ToString("C")}";
            }
        }

        public string SignedAmount => FinancialFlow == "I" ? $"+{Amount?.ToString("C")}" : $"-{Amount?.ToString("C")}";

        public string ConceptDescription
        {
            get
            {
                string resourceKey = null;

                switch (Concept)
                {
                    case 1:
                        resourceKey = "TrConcept_OrderPayment";
                        break;
                    case 2:
                        resourceKey = "TrConcept_ExtraExpend";
                        break;
                    case 3:
                        resourceKey = "TrConcept_SupplierOrder";
                        break;
                    default:
                        return "Concepto desconocido";
                }

                var resource = Application.Current.TryFindResource(resourceKey);
                return resource != null ? resource.ToString() : "Concepto no encontrado";
            }
        }
    }
}