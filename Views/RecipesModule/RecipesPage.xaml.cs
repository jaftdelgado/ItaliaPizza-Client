using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
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
                    PreparationTime = r.PreparationTime
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

        }
    }
}
