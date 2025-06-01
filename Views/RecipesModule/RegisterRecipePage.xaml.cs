using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.RecipesModule
{
    public partial class RegisterRecipePage : Page
    {
        private int _stepCounter = 1;
        private List<RecipeStep> _steps = new List<RecipeStep>();
        private List<RecipeSupplyItem> _ingredients = new List<RecipeSupplyItem>();
        private List<Supply> _allSupplies = new List<Supply>();
        private Product _associatedProduct;

        public RegisterRecipePage()
        {
            InitializeComponent();
            AddInitialStep();
            ConfigureDefaultView();
            Loaded += RegisterRecipePage_Loaded;
        }

        public RegisterRecipePage(Product product) : this()
        {
            _associatedProduct = product;
            LoadProductData();
            Loaded += RegisterRecipePage_Loaded;
        }

        private void RegisterRecipePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliesData();
        }

        private async void LoadSuppliesData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var supplyDTOs = client.GetAllSupplies(true);
                var supplies = supplyDTOs.Select(s => new Supply
                {
                    Id = s.Id,
                    Brand = s.Brand,
                    Name = s.Name,
                    MeasureUnit = s.MeasureUnit,
                    SupplyPic = s.SupplyPic,
                    SupplyCategoryID = s.SupplyCategoryID,
                    IsActive = s.IsActive
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _allSupplies = supplies;
                });
            });
        }

        private void LoadProductData()
        {
            if (_associatedProduct != null)
            {
                ProductName.Text = _associatedProduct.Name;
                ProductCode.Text = _associatedProduct.ProductCode;
                ProductCategory.Text = _associatedProduct.CategoryName;
                ProductDescription.Text = _associatedProduct.Description;

                if(_associatedProduct.ProductCode != null)
                    ProductCode.Text = "  •  " + _associatedProduct.ProductCode;
                else
                    ProductCode.Visibility = Visibility.Collapsed;

                ImageUtilities.SetImageSource(ProductPic, _associatedProduct.ProductPic, Constants.DEFAULT_PRODUCT_PIC_PATH);
            }
        }

        private void ConfigureDefaultView()
        {
            BtnAssociateRecipe.IsEnabled = false;
            UpdateButtonState();
        }

        private void AddInitialStep()
        {
            AddStep();
        }

        private void AddStep()
        {
            var stepControl = new RecipeStepControl();

            stepControl.StepNumber = _stepCounter++;
            stepControl.Instruction = "";
            stepControl.DeleteRequested += (s, e) => RemoveStep(stepControl);
            stepControl.InstructionChanged += (s, e) => UpdateButtonState();

            StepsContainer.Children.Add(stepControl);
            _steps.Add(new RecipeStep { StepNumber = stepControl.StepNumber, Instruction = "" });

            UpdateButtonState();
        }

        private void RemoveStep(RecipeStepControl stepControl)
        {
            int index = StepsContainer.Children.IndexOf(stepControl);
            if (index >= 0)
            {
                StepsContainer.Children.RemoveAt(index);
                _steps.RemoveAt(index);
                RenumberSteps();
                UpdateButtonState();
            }
        }

        private void RenumberSteps()
        {
            _stepCounter = 1;
            for (int i = 0; i < _steps.Count; i++)
            {
                _steps[i].StepNumber = _stepCounter;

                if (StepsContainer.Children[i] is RecipeStepControl ctrl) ctrl.StepNumber = _stepCounter;

                _stepCounter++;
            }
        }

        private void UpdateButtonState()
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                if (StepsContainer.Children[i] is RecipeStepControl ctrl)
                {
                    _steps[i].Instruction = ctrl.Instruction?.Trim() ?? "";
                    _steps[i].StepNumber = ctrl.StepNumber;
                }
            }

            bool allStepsFilled = _steps.All(s => !string.IsNullOrWhiteSpace(s.Instruction));
            bool allIngredientsHaveQuantity = _ingredients.All(i => i.UseQuantity > 0);

            BtnAssociateRecipe.IsEnabled = allStepsFilled && _steps.Count > 0
                && _ingredients.Count > 0 && allIngredientsHaveQuantity && _associatedProduct != null;
        }

        private void DisplayIngredients()
        {
            IngredientsContainer.Children.Clear();

            foreach (var item in _ingredients)
            {
                var ingredientDetail = new IngredientDetail
                {
                    IngredientName = item.Supply.Name,
                    Quantity = item.UseQuantity,
                    MeasureUnitId = item.Supply?.MeasureUnit ?? 0,
                    Margin = new Thickness(0, 0, 0, 6)
                };

                ImageUtilities.SetImageSource(ingredientDetail.IngredientPic, item.Supply.SupplyPic, Constants.DEFAULT_SUPPLY_PIC_PATH);

                IngredientsContainer.Children.Add(ingredientDetail);
            }
        }

        private void Click_BtnAddStep(object sender, RoutedEventArgs e)
        {
            AddStep();
        }

        private void Click_BtnAddIngredient(object sender, RoutedEventArgs e)
        {
            var selectSupply = new SelectSupply();
            selectSupply.SetSupplies(_allSupplies, _ingredients.Select(i => i.Supply));

            selectSupply.SupplySelectionChanged += (supply, isChecked) =>
            {
                if (isChecked)
                {
                    if (!_ingredients.Any(i => i.Supply.Id == supply.Id))
                    {
                        _ingredients.Add(new RecipeSupplyItem
                        {
                            Supply = supply,
                            SupplyID = supply.Id,
                            UseQuantity = 1.00m
                        });
                    }
                }
                else
                {
                    _ingredients.RemoveAll(i => i.Supply.Id == supply.Id);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisplayIngredients();
                    UpdateButtonState();
                });
            };

            selectSupply.Show(sender as FrameworkElement);
        }

        private void Click_BtnAssociateRecipe(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }
    }
}
