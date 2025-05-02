using System.Text.RegularExpressions;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class Supplier
    {
        public int Id { get; set; }

        public string SupplierName { get; set; }

        public string ContactName { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public string Description { get; set; }

        public int CategorySupply { get; set; }

        public SupplyCategory SupplyCategory
        {
            get
            {
                var category = SupplyCategory.GetDefaultSupplyCategories()
                    .Find(c => c.Id == CategorySupply);
                return category ?? new SupplyCategory(0, "Unknown");
            }
        }

        public string TranslatedCategory
        {
            get
            {
                var category = SupplyCategory;
                var translated = Application.Current.TryFindResource(category.ResourceKey) as string;
                return translated ?? category.Name;
            }
        }

        public string DisplayEmail => string.IsNullOrWhiteSpace(EmailAddress) ? "N/D" : EmailAddress;

        public string DisplayPhone
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PhoneNumber)) return "N/D";
                var digitsOnly = Regex.Replace(PhoneNumber, @"\D", "");
                return digitsOnly.Length == 10
                    ? $"{digitsOnly.Substring(0, 3)} {digitsOnly.Substring(3, 3)} {digitsOnly.Substring(6, 4)}"
                    : PhoneNumber;
            }
        }
    }
}
