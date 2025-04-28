using System.Web.Security;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class Supply
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int MeasureUnit { get; set; }

        public int SupplyCategoryID { get; set; }

        public string Brand { get; set; }

        public int? SupplierID { get; set; }

        public decimal Stock { get; set; }

        public byte[] SupplyPic { get; set; }

        public string Description { get; set; }

        public string FormattedPrice
        {
            get
            {
                return Price.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        public string SupplyName
        {
            get
            {
                if (!string.IsNullOrEmpty(Brand))
                {
                    return $"{Brand}® {Name}"; 
                }

                return Name;
            }
        }

        public string CategoryName
        {
            get
            {
                var category = SupplyCategory.GetDefaultSupplyCategories().Find(r => r.Id == SupplyCategoryID);

                var translated = Application.Current.TryFindResource(category.ResourceKey) as string;
                return translated ?? category.Name;
            }
        }


    }
}
