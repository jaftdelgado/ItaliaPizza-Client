using System.Collections.Generic;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceKey { get; set; }

        public ProductCategory(int id, string nameKeyBase)
        {
            Id = id;
            ResourceKey = $"Product_{nameKeyBase}";
            Name = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<ProductCategory> GetDefaultProductCategories()
        {
            return new List<ProductCategory>
            {
                new ProductCategory(1, "Appetizers"),
                new ProductCategory(2, "Salads"),
                new ProductCategory(3, "SavoryPizzas"),
                new ProductCategory(4, "SweetPizzas"),
                new ProductCategory(5, "Desserts"),
                new ProductCategory(6, "BeveragesAndCocktails")
            };
        }
    }
}
