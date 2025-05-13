using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views
{
    public partial class PersonalPage : Page
    {
        private List<Personal> _allPersonals = new List<Personal>();

        public PersonalPage()
        {
            InitializeComponent();
            BtnActive.Tag = "Selected";
            Loaded += PersonalPage_Loaded;
        }

        private void PersonalPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPersonalData();
        }

        private async void LoadPersonalData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dtoList = client.GetAllPersonals();

                var list = dtoList.Select(p => new Personal
                {
                    PersonalID = p.PersonalID,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    RFC = p.RFC,
                    EmailAddress = p.EmailAddress,
                    PhoneNumber = p.PhoneNumber,
                    Username = p.Username,
                    Password = p.Password,
                    ProfilePic = p.ProfilePic,
                    HireDate = p.HireDate,
                    IsActive = p.IsActive,
                    RoleID = p.RoleID,
                    AddressID = p.AddressID,
                    IsOnline = p.IsOnline,
                    Address = p.Address == null ? null : new Address
                    {
                        Id = p.Address.Id,
                        AddressName = p.Address.AddressName,
                        ZipCode = p.Address.ZipCode,
                        City = p.Address.City
                    }
                })
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

                _allPersonals = list;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyFilter("BtnActive");
                });
            });
        }

        private async Task DeleteEmployee(Personal selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool success = client.DeletePersonal(selected.PersonalID);
                if (!success) return;

                var item = _allPersonals.FirstOrDefault(p => p.PersonalID == selected.PersonalID);
                if (item != null)
                    item.IsActive = false;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    string selectedFilter = GetSelectedFilterButtonName();
                    ApplyFilter(selectedFilter);

                    MessageDialog.Show("Personal_DialogTDeletedEmployee", "Personal_DialogDDeletedEmployee", AlertType.SUCCESS);
                });
            });
        }

        private async Task ReactivateEmployee(Personal selected)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.ReactivatePersonal(selected.PersonalID);
                if (!result) return;

                var item = _allPersonals.FirstOrDefault(p => p.PersonalID == selected.PersonalID);
                if (item != null)
                {
                    item.IsActive = true;
                    item.HireDate = DateTime.Now;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (!_allPersonals.Any(p => !p.IsActive))
                        ApplyFilter("BtnActive");
                    else
                        ApplyFilter(GetSelectedFilterButtonName());

                    DisplayEmployeeDetails(selected);
                    MessageDialog.Show("Personal_DialogTReactivatedEmployee", "Personal_DialogDReactivatedEmployee", AlertType.SUCCESS);
                });
            });
        }

        private void ApplyFilter(string buttonName)
        {
            IEnumerable<Personal> filteredList = _allPersonals;

            switch (buttonName)
            {
                case "BtnActive":
                    filteredList = _allPersonals.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = _allPersonals.Where(p => !p.IsActive);
                    break;
            }

            EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    EmptyListMessage.Visibility = Visibility.Visible;
                else
                    NoMatchesMessage.Visibility = Visibility.Visible;
            }

            PersonalDataGrid.ItemsSource = filteredList;

            BtnActive.Tag = null;
            BtnDeleted.Tag = null;
            BtnViewAll.Tag = null;

            switch (buttonName)
            {
                case "BtnActive":
                    BtnActive.Tag = "Selected";
                    break;
                case "BtnDeleted":
                    BtnDeleted.Tag = "Selected";
                    break;
                case "BtnViewAll":
                    BtnViewAll.Tag = "Selected";
                    break;
            }

            BtnDeleted.IsEnabled = _allPersonals.Any(p => !p.IsActive);
            UpdateElementsCounter(filteredList.Count());
        }

        private void DisplayEmployeeDetails(Personal selected)
        {
            if (selected == null) return;

            UpdateEmployeePanelVisibility(selected);

            ImageUtilities.SetImageSource(EmployeeProfilePic, selected.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);

            EmployeeName.Text = selected.FullName;
            EmployeeUsername.Text = selected.DisplayUsername;
            EmployeeRole.Text = selected.TranslatedRole;
            EmployeePhone.Text = selected.PhoneNumber;
            EmployeeEmail.Text = selected.EmailAddress;

            string hiredOnText = FindResource("Personal_HiredOn") as string ?? "";
            EmployeeHireDate.Text = $"{hiredOnText.Trim()} {selected.HireDate:dd/MM/yyyy}";

            EmployeeAddress.Text = selected.FullAddress;

            BtnDeleteEmployee.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnEditEmployee.Visibility = selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
            BtnReactivateEmployee.Visibility = !selected.IsActive ? Visibility.Visible : Visibility.Collapsed;
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnViewAll";
        }

        private void UpdateEmployeePanelVisibility(Personal selected)
        {
            bool hasSelection = selected != null;

            EmployeeDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateElementsCounter(int count)
        {
            ElementsCounter.Content = count.ToString();
        }

        private void Click_BtnNewEmployee(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
                mainWindow.NavigateToPage("RegEmployee_Header", new RegisterEmployeePage());
        }

        private void PersonalDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalDataGrid.SelectedItem is Personal selected)
                DisplayEmployeeDetails(selected);
            else
                UpdateEmployeePanelVisibility(null);
        }

        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();

            string selectedFilter = GetSelectedFilterButtonName();

            IEnumerable<Personal> filteredList = _allPersonals;

            switch (selectedFilter)
            {
                case "BtnActive":
                    filteredList = filteredList.Where(p => p.IsActive);
                    break;
                case "BtnDeleted":
                    filteredList = filteredList.Where(p => !p.IsActive);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredList = filteredList.Where(p =>
                    $"{p.FirstName} {p.LastName}".ToLower().Contains(searchText) ||
                    p.RFC?.ToLower().Contains(searchText) == true ||
                    p.PhoneNumber?.ToLower().Contains(searchText) == true ||
                    p.Address != null && (
                        p.Address.AddressName?.ToLower().Contains(searchText) == true ||
                        p.Address.City?.ToLower().Contains(searchText) == true ||
                        p.Address.ZipCode?.ToLower().Contains(searchText) == true
                    )
                );
            }

            EmptyListMessage.Visibility = Visibility.Collapsed;
            NoMatchesMessage.Visibility = Visibility.Collapsed;

            if (!filteredList.Any())
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    EmptyListMessage.Visibility = Visibility.Visible;
                else
                    NoMatchesMessage.Visibility = Visibility.Visible;
            }

            PersonalDataGrid.ItemsSource = filteredList;
            UpdateElementsCounter(filteredList.Count());
        }

        private void Click_BtnDeleteEmployee(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Personal_DialogTDeleteEmployee", "Personal_DialogDDeleteEmployee",
                async () =>
                {
                    if (PersonalDataGrid.SelectedItem is Personal selected)
                        await DeleteEmployee(selected);
                },
                "Glb_Delete"
            );
        }

        private void Click_BtnReactivateEmployee(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Personal_DialogTReactivateEmployee", "Personal_DialogDReactivateEmployee",
                async () =>
                {
                    if (PersonalDataGrid.SelectedItem is Personal selected)
                        await ReactivateEmployee(selected);
                }
            );
        }

        private void Click_BtnEditEmployee(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            var employeeToEdit = PersonalDataGrid.SelectedItem as Personal;

            if (mainWindow != null && employeeToEdit != null)
                mainWindow.NavigateToPage("EditEmployee_Header", new RegisterEmployeePage(employeeToEdit));
        }
    }

}
