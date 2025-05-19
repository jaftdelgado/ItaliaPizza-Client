using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.RecipesModule;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
                    ProductID = r.ProductID,
                    ProductName = r.ProductName,
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

            RecipeDescription.Text = selected.Description;

            RecipePreparationTime.Inlines.Clear();
            RecipePreparationTime.Inlines.Add(new Run { FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Text = "\uE823" });
            RecipePreparationTime.Inlines.Add(new Run { Text = " " + selected.PreparationTime + " minutos" });
            ProductRecipeTitle.Text = "Receta del producto:\n"+ selected.ProductName;
        }

        private void Click_BtnShowRecipe(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null && recipesDataGrid.SelectedItem is Recipe selected)
                mainWindow.NavigateToPage("Recipes_BtnEditRecipe", new RecipeRegister(selected));
        }
    }
}
