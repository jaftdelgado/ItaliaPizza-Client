using ItaliaPizzaClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ItaliaPizzaClient.Model;

namespace ItaliaPizzaClient.Views.RecepiesModule
{
    public partial class RecipesPage : Page
    {
        private List<Product> _productsWithRecipes;

        public RecipesPage()
        {
            InitializeComponent();
            Loaded += RecipesPage_Loaded;
        }

        private void RecipesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProductsWithRecipes();
        }

        private async void LoadProductsWithRecipes()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllProducts(false);

                var list = dtoList
                    .Where(p => p.Recipe != null)
                    .Select(p => new Product
                    {
                        ProductID = p.ProductID,
                        Name = p.Name,
                        Category = p.Category,
                        Price = p.Price,
                        IsPrepared = p.IsPrepared,
                        ProductPic = p.ProductPic,
                        Description = p.Description,
                        ProductCode = p.ProductCode,
                        IsActive = p.IsActive,
                        SupplyID = p.SupplyID,
                        IsDeletable = p.IsDeletable,
                        RecipeID = p.RecipeID,

                        Recipe = new Recipe
                        {
                            Id = p.Recipe.RecipeID,
                            PreparationTime = p.Recipe.PreparationTime,
                            Steps = p.Recipe.Steps?.Select(rs => new RecipeStep
                            {
                                Id = rs.RecipeStepID,
                                RecipeID = rs.RecipeID,
                                StepNumber = rs.StepNumber,
                                Instruction = rs.Instruction
                            }).OrderBy(rs => rs.StepNumber).ToList() ?? new List<RecipeStep>(),

                            Supplies = p.Recipe.Supplies?.Select(rsp => new RecipeSupplyItem
                            {
                                Id = rsp.RecipeSupplyID,
                                RecipeID = rsp.RecipeID,
                                SupplyID = rsp.SupplyID,
                                UseQuantity = rsp.UseQuantity
                            }).ToList() ?? new List<RecipeSupplyItem>()
                        }
                    })
                    .OrderBy(p => p.Name)
                    .ToList();

                _productsWithRecipes = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DisplayRecipeProducts(_productsWithRecipes);
                });
            });
        }

        private void DisplayRecipeProducts(List<Product> products)
        {
            ProductsContainer.Children.Clear();

            foreach (var product in products)
            {
                var button = new Button
                {
                    Style = (Style)FindResource("ProductCardButtonStyle"),
                    Margin = new Thickness(20, 5, 20, 5),
                    Cursor = Cursors.Hand
                };

                button.Click += ProductRecipeButton_Click;

                button.Loaded += (s, e) =>
                {
                    if (s is Button btn && btn.Tag is Product prod)
                    {
                        if (btn.Template.FindName("PART_Image", btn) is Image image)
                        {
                            image.Source = ImageUtilities.ConvertToImageSource(prod.ProductPic);
                        }

                        if (btn.Template.FindName("PART_Name", btn) is TextBlock nameBlock)
                        {
                            nameBlock.Text = prod.Name;
                        }

                        if (btn.Template.FindName("PART_Category", btn) is TextBlock categoryBlock)
                        {
                            categoryBlock.Text = prod.CategoryName;
                        }
                    }
                };


                ProductsContainer.Children.Add(button);
            }
        }

        private void ProductRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                // Mostrar información del producto
                ProductName.Text = product.Name;
                ProductCategory.Text = product.CategoryName;
                ProductCode.Text = $" | {product.ProductCode}";
                ProductDescription.Text = product.Description;

                // Imagen del producto
                ProductPic.Source = ImageUtilities.ConvertToImageSource(product.ProductPic);

                // Cargar ingredientes
                IngredientsContainer.Items.Clear();
                foreach (var ingredient in product.Recipe.Supplies)
                {
                    var item = new TextBlock
                    {
                        Style = (Style)FindResource("RegularLabelStyle"),
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    IngredientsContainer.Items.Add(item);
                }

                // Cargar pasos
                StepsContainer.Children.Clear();
                foreach (var step in product.Recipe.Steps)
                {
                    var stepText = new TextBlock
                    {
                        Text = $"{step.StepNumber}. {step.Instruction}",
                        TextWrapping = TextWrapping.Wrap,
                        Style = (Style)FindResource("DescriptionLabelStyle"),
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    StepsContainer.Children.Add(stepText);
                }
            }
        }

    }
}
