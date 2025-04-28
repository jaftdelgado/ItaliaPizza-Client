using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using ItaliaPizzaClient.Views;
using System.Windows.Data;
using ItaliaPizzaClient.Views.CustomersModule;
using ItaliaPizzaClient.Views.SuppliersModule;
using ItaliaPizzaClient.Views.OrdersModule;

namespace ItaliaPizzaClient.Utilities
{
    public class MenuButton : DependencyObject
    {
        private string _content;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                FindName();
            }
        }

        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register("IconPath", typeof(string), typeof(MenuButton));

        public string IconPath
        {
            get => (string)GetValue(IconPathProperty);
            set => SetValue(IconPathProperty, value);
        }

        public static readonly DependencyProperty HoverIconPathProperty =
            DependencyProperty.Register("HoverIconPath", typeof(string), typeof(MenuButton));

        public string HoverIconPath
        {
            get => (string)GetValue(HoverIconPathProperty);
            set => SetValue(HoverIconPathProperty, value);
        }

        public List<string> AllowedRoles { get; set; }
        public int Order { get; set; }
        public string ResourceName { get; set; }
        public Page PageInstance { get; set; }

        public bool CanAccess(string role) => AllowedRoles.Contains(role);

        private void FindName()
        {
            if (!string.IsNullOrEmpty(ResourceName))
            {
                var resourceDictionary = Application.Current.Resources;
                if (resourceDictionary[ResourceName] is string resourceValue)
                {
                    _content = resourceValue;
                }
            }
        }
    }

    public class SideMenuManager
    {
        public static string CurrentUserRole;
        private NavigationManager navigationManager;
        private Button lastSelectedButton;

        public SideMenuManager(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public static List<MenuButton> GetButtonsForRole(string role)
        {
            var buttons = new List<MenuButton>
            {
                new MenuButton {
                    ResourceName = "Glb_Principal",
                    Order = 1,
                    AllowedRoles = new List<string> { "Admin", "Manager", "Cashier", "Waiter", "Cook" },
                    IconPath = "/Resources/Icons/home-icon.png",
                    HoverIconPath = "/Resources/Icons/home-hover-icon.png",
                    PageInstance = new PrincipalPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Personal",
                    Order = 2,
                    AllowedRoles = new List<string> { "Admin" },
                    IconPath = "/Resources/Icons/personal-icon.png",
                    HoverIconPath = "/Resources/Icons/personal-hover-icon.png",
                    PageInstance = new PersonalPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Orders",
                    Order = 2,
                    AllowedRoles = new List<string> { "Cashier", "Waiter", "Cook" },
                    IconPath = "/Resources/Icons/orders-icon.png",
                    HoverIconPath = "/Resources/Icons/orders-hover-icon.png",
                    PageInstance = new OrderPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Products",
                    Order = 3,
                    AllowedRoles = new List<string> { "Admin", "Cashier", "Waiter", "Cook" },
                    IconPath = "/Resources/Icons/products-icon.png",
                    HoverIconPath = "/Resources/Icons/products-hover-icon.png",
                    PageInstance = new ProductsPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Recipes",
                    Order = 4,
                    AllowedRoles = new List<string> { "Admin", "Cook" },
                    IconPath = "/Resources/Icons/recipes-icon.png",
                    HoverIconPath = "/Resources/Icons/recipes-hover-icon.png"
                },
                new MenuButton
                {
                    ResourceName = "Glb_Supplies",
                    Order = 5,
                    AllowedRoles = new List<string> { "Admin", "Manager" },
                    IconPath = "/Resources/Icons/supplies-icon.png",
                    HoverIconPath = "/Resources/Icons/supplies-hover-icon.png",
                    PageInstance = new SuppliesPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Stock",
                    Order = 6,
                    AllowedRoles = new List<string> { "Admin", "Manager" },
                    IconPath = "/Resources/Icons/stock-icon.png",
                    HoverIconPath = "/Resources/Icons/stock-hover-icon.png"
                },
                new MenuButton
                {
                    ResourceName = "Glb_Suppliers",
                    Order = 7,
                    AllowedRoles = new List<string> { "Admin", "Manager", "Cashier" },
                    IconPath = "/Resources/Icons/suppliers-icon.png",
                    HoverIconPath = "/Resources/Icons/suppliers-hover-icon.png",
                    PageInstance = new SuppliersPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_CashRegister",
                    Order = 2,
                    AllowedRoles = new List<string> { "Manager", "Cashier" },
                    IconPath = "/Resources/Icons/cashregister-icon.png",
                    HoverIconPath = "/Resources/Icons/cashregister-hover-icon.png"
                },
                new MenuButton
                {
                    ResourceName = "Glb_Clients",
                    Order = 10,
                    AllowedRoles = new List<string> { "Cashier" },
                    IconPath = "/Resources/Icons/clients-icon.png",
                    HoverIconPath = "/Resources/Icons/clients-hover-icon.png",
                    PageInstance = new CustomersPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_SupplyOrders",
                    Order = 11,
                    AllowedRoles = new List<string> { "Admin", "Manager", "Cashier" },
                    IconPath = "/Resources/Icons/supplyorders-icon.png",
                    HoverIconPath = "/Resources/Icons/supplyorders-hover-icon.png",
                    PageInstance = new OrderSuppliersPage()
                },
                new MenuButton
                {
                    ResourceName = "Glb_Reports",
                    Order = 12,
                    AllowedRoles = new List<string> { "Admin" },
                    IconPath = "/Resources/Icons/reports-icon.png",
                    HoverIconPath = "/Resources/Icons/reports-hover-icon.png"
                }
            };

            return buttons.Where(b => b.CanAccess(role)).OrderBy(b => b.Order).ToList();
        }

        public void LoadButtons(StackPanel stackPanel)
        {
            stackPanel.Children.Clear();
            var buttons = GetButtonsForRole(CurrentUserRole);

            foreach (var buttonInfo in buttons)
            {
                var button = new Button
                {
                    Content = Application.Current.FindResource(buttonInfo.ResourceName),
                    Style = (Style)Application.Current.FindResource("MenuButtonStyle"),
                };

                button.DataContext = buttonInfo;

                button.SetBinding(MenuButton.IconPathProperty, new Binding("IconPath"));
                button.SetBinding(MenuButton.HoverIconPathProperty, new Binding("HoverIconPath"));

                SideMenuButtonHelper.SetIsSelected(button, false);

                button.Click += (sender, e) =>
                {
                    if (IsCurrentModule(buttonInfo)) return;

                    if (lastSelectedButton != null)
                        SideMenuButtonHelper.SetIsSelected(lastSelectedButton, false);

                    SideMenuButtonHelper.SetIsSelected(button, true);
                    lastSelectedButton = button;

                    button.IsEnabled = false;
                    navigationManager.ClearNavigationStack();
                    NavigateToPage(buttonInfo);
                    button.Dispatcher.Invoke(() => button.IsEnabled = true);
                };

                stackPanel.Children.Add(button);
            }
        }


        private bool IsCurrentModule(MenuButton buttonInfo)
        {
            var currentModuleName = navigationManager.navigationStack.LastOrDefault()?.Name;
            return currentModuleName == Application.Current.Resources[buttonInfo.ResourceName] as string;
        }

        private void NavigateToPage(MenuButton buttonInfo)
        {
            navigationManager.NavigateToPage(buttonInfo.ResourceName, buttonInfo.PageInstance);
        }
    }
}