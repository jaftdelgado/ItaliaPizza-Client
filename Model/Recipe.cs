using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItaliaPizzaClient.Model
{
    public class Recipe
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int PreparationTime { get; set; }

        public Product Product { get; set; }

        public RecipeSupplyItem RecipeSupplyItem { get; set; }

    }
}
