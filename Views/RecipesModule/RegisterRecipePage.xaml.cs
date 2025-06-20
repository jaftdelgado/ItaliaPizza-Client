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
        
        public event Action<Recipe> RecipeAssociated;

        private Recipe _loadedRecipe;

        public RegisterRecipePage()
        {
            InitializeComponent();
            AddInitialStep();
            ConfigureDefaultView();
            Loaded += RegisterRecipePage_Loaded;
            InputUtilities.ValidateInput(TbPreparationTime, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_TABLE);
        }

        public RegisterRecipePage(Product product, List<Supply> availableSupplies) : this()
        {
            _associatedProduct = product;
            _allSupplies = availableSupplies;
            LoadProductData();
            InputUtilities.ValidateInput(TbPreparationTime, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_TABLE);
        }

        public RegisterRecipePage(Product product, Recipe recipe, List<Supply> availableSupplies) : this(product, availableSupplies)
        {
            _loadedRecipe = recipe;
            InputUtilities.ValidateInput(TbPreparationTime, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_TABLE);
        }

        private void RegisterRecipePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loadedRecipe != null)
            {
                LoadRecipeData(_loadedRecipe);
            }
        }

        private void LoadRecipeData(Recipe recipe)
        {
            if (recipe == null) return;

            TbPreparationTime.Text = recipe.PreparationTime.ToString();

            _steps.Clear();
            StepsContainer.Children.Clear();
            foreach (var step in recipe.Steps.OrderBy(s => s.StepNumber))
            {
                var stepControl = new RecipeStepControl
                {
                    StepNumber = step.StepNumber,
                    Instruction = step.Instruction
                };
                StepsContainer.Children.Add(stepControl);
                _steps.Add(new RecipeStep { StepNumber = step.StepNumber, Instruction = step.Instruction });
            }

            _stepCounter = recipe.Steps.Max(s => s.StepNumber) + 1;

            _ingredients.Clear();

            foreach (var supply in recipe.Supplies)
            {
                var matchingSupply = _allSupplies.FirstOrDefault(s => s.Id == supply.SupplyID);
                if (matchingSupply != null)
                {
                    var ingredient = new RecipeSupplyItem
                    {
                        SupplyID = supply.SupplyID,
                        UseQuantity = supply.UseQuantity,
                        Supply = matchingSupply
                    };

                    _ingredients.Add(ingredient);
                }
            }

            DisplayIngredients(); 
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
            IngredientsContainer.Items.Clear();

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

                ingredientDetail.QuantityChanged += (s, newQuantity) =>
                {
                    item.UseQuantity = newQuantity;
                    UpdateButtonState();
                };

                IngredientsContainer.Items.Add(ingredientDetail);
            }
        }

        private void Click_BtnAddStep(object sender, RoutedEventArgs e)
        {
            AddStep();
        }

        private void Click_BtnAddIngredient(object sender, RoutedEventArgs e)
        {
            var selectSupply = new SelectSupply
            {
                IsSingleSelection = false
            };

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
            var recipe = new Recipe
            {
                Product = _associatedProduct,
                ProductID = _associatedProduct.ProductID,
                PreparationTime = int.TryParse(TbPreparationTime.Text, out int prepTime) ? prepTime : 0,
                Steps = _steps,
                Supplies = _ingredients
            };

            RecipeAssociated?.Invoke(recipe);
            NavigationManager.Instance.GoBack();
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }
    }
}
