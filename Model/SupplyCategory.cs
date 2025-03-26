

using System.Collections.Generic;

namespace ItaliaPizzaClient.Model
{
    public class SupplyCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SupplyCategory(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<SupplyCategory> GetDefaultSupplyCategories()
        {
            return new List<SupplyCategory>
        {
            new SupplyCategory(1, "Frutas y Verduras"),
            new SupplyCategory(2, "Carnes Frías"),
            new SupplyCategory(3, "Salsas y Aderezos"),
            new SupplyCategory(4, "Condimentos y Especias")
        };
        }
    }
}
