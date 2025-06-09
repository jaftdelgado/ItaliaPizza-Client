using System.Collections.Generic;

namespace ItaliaPizzaClient.Model
{
    public class Recipe
    {
        public int Id { get; set; }

        public int PreparationTime { get; set; }

        public Product Product { get; set; }
        public int ProductID { get; set; }

        public List<RecipeStep> Steps { get; set; } = new List<RecipeStep>();

        public List<RecipeSupplyItem> Supplies { get; set; } = new List<RecipeSupplyItem>();
    }

    public class RecipeStep
    {
        public int Id { get; set; } 

        public int RecipeID { get; set; }

        public int StepNumber { get; set; }

        public string Instruction { get; set; }
    }

    public class RecipeSupplyItem
    {
        public int Id { get; set; }

        public int RecipeID { get; set; }

        public int SupplyID { get; set; }

        public decimal UseQuantity { get; set; }

        public Supply Supply { get; set; }
    }
}
