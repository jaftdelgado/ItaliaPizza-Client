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
            Loaded += PersonalPage_Loaded;
        }

        private async void PersonalPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPersonalDataAsync();
        }

        private async Task LoadPersonalDataAsync()
        {
            // Mostrar loading en el hilo de UI
            LoadingText.Visibility = Visibility.Visible;
            PersonalDataGrid.ItemsSource = null;

            var client = ConnectionUtilities.IsServerConnected();
            if (client == null)
            {
                LoadingText.Visibility = Visibility.Collapsed;
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    var dtoList = client.GetAllPersonals();

                    _allPersonals = dtoList.Select(p => new Personal
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


                    Dispatcher.Invoke(() =>
                    {
                        ApplyFilter("BtnActive");
                    });
                }
                catch (Exception)
                {
                    // Mostrar el mensaje en el hilo de UI
                    Dispatcher.Invoke(() =>
                    {
                        ConnectionUtilities.ShowSafeDialog("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                    });
                }
                finally
                {
                    // Siempre ocultar el loading al final
                    Dispatcher.Invoke(() =>
                    {
                        LoadingText.Visibility = Visibility.Collapsed;
                    });
                }
            });
        }

        private void DeleteEmployee(Personal selected)
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            ConnectionUtilities.ExecuteDatabaseSafeAction(() =>
            {
                bool result = client.DeletePersonal(selected.PersonalID);
                if (result)
                {
                    var item = _allPersonals.FirstOrDefault(p => p.PersonalID == selected.PersonalID);
                    if (item != null)
                        item.IsActive = false;

                    Dispatcher.Invoke(() =>
                    {
                        string selectedFilter = GetSelectedFilterButtonName();
                        ApplyFilter(selectedFilter);

                        Dispatcher.InvokeAsync(() =>
                        {
                            MessageDialog.Show("Personal_DialogTDeletedEmployee", "Personal_DialogDDeletedEmployee", AlertType.SUCCESS);
                        });
                    });

                }
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
                case "BtnViewAll":
                default:
                    break;
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
        }

        private string GetSelectedFilterButtonName()
        {
            if (BtnActive.Tag?.ToString() == "Selected") return "BtnActive";
            if (BtnDeleted.Tag?.ToString() == "Selected") return "BtnDeleted";
            return "BtnViewAll";
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
            {
                ImageUtilities.SetImageSource(EmployeeProfilePic, selected.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);

                EmployeeRFC.Text = selected.RFC;
                EmployeeLastName.Text = selected.LastName;
                EmployeeFirstName.Text = selected.FirstName;
                EmployeeRole.Text = selected.TranslatedRole;
                EmployeePhone.Text = selected.PhoneNumber;
                EmployeeEmail.Text = selected.EmailAddress;

                string hiredOnText = FindResource("Personal_HiredOn") as string ?? null;
                EmployeeHireDate.Text = $"{hiredOnText}{selected.HireDate:dd/MM/yyyy}";

                EmployeeAddress.Text = selected.FullAddress;
            }
        }

        private void Click_BtnDeleteEmployee(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "Personal_DialogTDeleteEmployee", "Personal_DialogDDeleteEmployee",
                () =>
                {
                    if (PersonalDataGrid.SelectedItem is Personal selected)
                        DeleteEmployee(selected);
                },
                "Glb_Delete"
            );
        }


        private void Click_FilterButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) ApplyFilter(button.Name);
        }


    }

}
