using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.RecipesModule;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.RecepiesModule
{
    public partial class RecipesPage : Page
    {

        private List<Recipe> _allRecipes = new List<Recipe>();
        private List<Recipe> _filteredRecipes = new List<Recipe>();
        public RecipesPage()
        {
            InitializeComponent();
            Loaded += RecipesPage_Loaded;
        }

        private void RecipesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecipesData();
        }

        private async void LoadRecipesData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetRecipes();

                var list = dtoList.Select(r => new Recipe
                {
                    Id = r.RecipeID,
                    Description = r.Description,
                    PreparationTime = r.PreparationTime,
                    Product = r.Product == null ? null : new Product
                    {
                        ProductID = r.Product.Id,
                        Name = r.Product.Name,
                        Photo = r.Product.Photo
                    }
                })
                .OrderBy(r => r.Id)
                .ToList();

                _allRecipes = list;
                _filteredRecipes = new List<Recipe>(_allRecipes);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    recipesDataGrid.ItemsSource = _filteredRecipes;
                });
            });
        }

        private void Clic_BtnNewRecipe(object sender, RoutedEventArgs e)
        {
        }

        private void recipesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (recipesDataGrid.SelectedItem is Recipe selected)
                DisplayRecipeDetails(selected);
            else
                UpdateRecipePanelVisibility(null);
        }

        private void UpdateRecipePanelVisibility(Recipe selected)
        {
            bool hasSelection = selected != null;

            RecipeDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DisplayRecipeDetails(Recipe selected)
        {
            if (selected == null)
                return;

            UpdateRecipePanelVisibility(selected);
        }
    }
}
