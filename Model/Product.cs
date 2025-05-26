using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class Product
    {
        public int ProductID { get; set; }

        public string Name { get; set; }

        public int? Category { get; set; }

        public decimal? Price { get; set; }

        public bool? IsPrepared { get; set; }

        public byte[] ProductPic { get; set; }

        public string Description { get; set; }

        public string ProductCode { get; set; }

        public bool IsActive { get; set; }

        public int? SupplyID { get; set; }

        public bool IsDeletable { get; set; }

        public string FormattedPrice
        {
            get
            {
                return Price.HasValue
                    ? Price.Value.ToString("C", System.Globalization.CultureInfo.CurrentCulture)
                    : "Sin precio";
            }
        }

        public string CategoryName
        {
            get
            {
                var category = ProductCategory.GetDefaultProductCategories().Find(c => c.Id == Category.Value);

                return category.Name;
            }
        }
    }
}
