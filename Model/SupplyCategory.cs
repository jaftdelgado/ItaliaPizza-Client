using System.Collections.Generic;
using System.Windows;

namespace ItaliaPizzaClient.Model
{
    public class SupplyCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResourceKey { get; set; }

        public SupplyCategory(int id, string nameKeyBase)
        {
            Id = id;
            ResourceKey = $"Supply_{nameKeyBase}";
            Name = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<SupplyCategory> GetDefaultSupplyCategories()
        {
            return new List<SupplyCategory>
            {
                new SupplyCategory(1, "Dairy"),
                new SupplyCategory(2, "Meats"),
                new SupplyCategory(3, "FruitsAndVegetables"),
                new SupplyCategory(4, "FloursAndDough"),
                new SupplyCategory(5, "SaucesAndDressings"),
                new SupplyCategory(6, "OilsAndVinegars"),
                new SupplyCategory(7, "SeasoningsAndSpices"),
                new SupplyCategory(8, "NutsAndSeeds"),
                new SupplyCategory(9, "Beverages"),
                new SupplyCategory(10, "DessertsAndSweets"),
                new SupplyCategory(11, "BakeryProducts")
            };
        }
    }
}
