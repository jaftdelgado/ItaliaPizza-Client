using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterEmployeePage : Page
    {
        private Personal _editingEmployee;
        private bool _isAccountVisible = false;
        private int _selectedRoleId = -1;
        private bool _isEditMode;

        public RegisterEmployeePage()
        {
            InitializeComponent();
            ConfigureInterfaceForMode();
            SetRolesComboBox();
            SetInputFields();
            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);

            ImageUtilities.SetImageSource(EmployeeProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
            _isEditMode = false;
        }

        public RegisterEmployeePage(Personal editingEmployee)
        {
            InitializeComponent();
            _isEditMode = true;
            _editingEmployee = editingEmployee;
            _selectedRoleId = editingEmployee.RoleID - 1;

            ConfigureInterfaceForMode();
            SetRolesComboBox();
            SetInputFields();
            LoadEmployeeData(editingEmployee);
            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);

            ImageUtilities.SetImageSource(EmployeeProfilePic, editingEmployee.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);

            if (editingEmployee.ProfilePic != null) BtnDeleteImage.IsEnabled = true;
        }

        private void ConfigureInterfaceForMode()
        {
            if (_isEditMode)
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "EditEmployee_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "EditEmployee_Desc");
                BtnEditEmployee.Visibility = Visibility.Visible;
                BtnRegisterEmployee.Visibility = Visibility.Collapsed;

                AccountBorder.Visibility = Visibility.Collapsed;
                _isAccountVisible = false;
            }
            else
            {
                PageHeader.SetResourceReference(TextBlock.TextProperty, "RegEmployee_Header");
                PageDescription.SetResourceReference(TextBlock.TextProperty, "RegEmployee_Desc");
                BtnEditEmployee.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadEmployeeData(Personal employee)
        {
            TbEmployeeName.Text = employee.FirstName;
            TbLastName.Text = employee.LastName;
            TbRFC.Text = employee.RFC;
            TbEmailAddress.Text = employee.EmailAddress;
            TbPhoneNumber.Text = employee.PhoneNumber;
            TbAddress.Text = employee.Address.AddressName;
            TbZipCode.Text = employee.Address.ZipCode;
            TbCity.Text = employee.Address.City;
            CbEmployeeRoles.SelectedIndex = _selectedRoleId;
        }

        private void SetRolesComboBox()
        {
            var roles = EmployeeRole.GetDefaultEmployeeRoles();

            foreach (var role in roles)
                role.Name = Application.Current.Resources[role.ResourceKey]?.ToString() ?? role.Name;

            CbEmployeeRoles.ItemsSource = roles;
        }

        private void SetInputFields()
        {
            var validations = new (TextBox, string, int)[]
            {
                (TbEmployeeName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbLastName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES),
                (TbPhoneNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_PHONENUMBER),
                (TbEmailAddress, Constants.EMAIL_ALLOWED_CHARS_PATTERN, Constants.MAX_LENGTH_EMAIL),
                (TbAddress, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_ADDRESSNAME),
                (TbZipCode, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_ZIPCODE),
                (TbCity, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_CITY),
                (TbRFC, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_RFC),
                (TbUsername, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_USERNAME),
                (TbPassword, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_PASSWORD)
            };

            foreach (var (textBox, pattern, maxLength) in validations)
                InputUtilities.ValidateInput(textBox, pattern, maxLength);

            InputUtilities.ValidatePasswordInput(PbPassword, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_PASSWORD);
            InputUtilities.ConvertToUpperCase(TbRFC);
            InputUtilities.ConvertToLowerCase(TbEmailAddress);
            InputUtilities.ConvertToLowerCase(TbUsername);
        }

        private void SelectProfileImage(Image targetImageControl)
        {
            var dialogTitle = Application.Current.Resources["RegEmployee_DialogSelectProfilePic"]?.ToString();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Constants.IMAGE_FILE_FILTER,
                Title = dialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var processedImageBytes = ImageUtilities.ProcessImageBeforeSaving(openFileDialog.FileName);

                    if (!ImageUtilities.IsImageSizeValid(processedImageBytes))
                    {
                        MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                        return;
                    }

                    EmployeeProfilePic.Source = ImageUtilities.ConvertToImageSource(processedImageBytes);
                    BtnDeleteImage.IsEnabled = true;
                }
                catch (Exception)
                {
                    MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                }
            }
        }

        private void UpdateFormButtonState(Button button)
        {
            var requiredFields = new List<TextBox>
            {
                TbRFC, TbEmployeeName, TbLastName, TbEmailAddress, TbPhoneNumber,
                TbAddress, TbZipCode, TbCity
            };

            if (!_isEditMode && _selectedRoleId != 6)
            {
                requiredFields.Add(TbUsername);
                requiredFields.Add(TbPassword);
            }

            bool allFieldsFilled = true;

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    allFieldsFilled = false;
                    break;
                }
            }

            button.IsEnabled = allFieldsFilled;
        }

        private void ToggleAccountVisibility(bool showAccount)
        {
            if (showAccount && !_isAccountVisible)
            {
                AccountBorder.Visibility = Visibility.Visible;
                Animations.BeginAnimation(AccountBorder, "ShowAccountBorderAnimation");
                _isAccountVisible = true;
            }
            else if (!showAccount && _isAccountVisible)
            {
                Animations.BeginAnimation(AccountBorder, "HideAccountBorderAnimation", () =>
                {
                    AccountBorder.Visibility = Visibility.Collapsed;
                    _isAccountVisible = false;
                });
            }
            else if (!showAccount)
                AccountBorder.Visibility = Visibility.Collapsed;
        }

        public byte[] GetProfilePicData()
        {
            byte[] profilePicData = null;

            if (EmployeeProfilePic.Source is BitmapImage bitmapImage)
            {
                profilePicData = ImageUtilities.ImageToByteArray(bitmapImage);

                var defaultImage = new BitmapImage(new Uri(Constants.DEFAULT_PROFILE_PIC_PATH, UriKind.Absolute));
                var defaultImageBytes = ImageUtilities.ImageToByteArray(defaultImage);

                if (ImageUtilities.AreImagesEqual(profilePicData, defaultImageBytes))
                {
                    profilePicData = null;
                }
            }

            return profilePicData;
        }

        public async Task RegisterEmployee()
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return;

            byte[] profilePicData = GetProfilePicData();

            var personalDto = new PersonalDTO
            {
                FirstName = TbEmployeeName.Text.Trim(),
                LastName = TbLastName.Text.Trim(),
                RFC = TbRFC.Text.Trim(),
                EmailAddress = TbEmailAddress.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                Username = string.IsNullOrWhiteSpace(TbUsername.Text) ? null : TbUsername.Text.Trim(),
                Password = (_selectedRoleId != 6 && !string.IsNullOrWhiteSpace(PbPassword.Password))
                    ? PasswordUtilities.HashPassword(PbPassword.Password) : null,
                ProfilePic = profilePicData,
                RoleID = _selectedRoleId,
                Address = new AddressDTO
                {
                    AddressName = TbAddress.Text.Trim(),
                    ZipCode = TbZipCode.Text.Trim(),
                    City = TbCity.Text.Trim()
                }
            };

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                if (_selectedRoleId != 6 && !IsUsernameAvailable(personalDto.Username)) return;
                if (!IsEmailAvailable(personalDto.EmailAddress)) return;
                if (!IsRfcUnique(personalDto.RFC)) return;

                int result = await client.AddPersonalAsync(personalDto);
                success = result > 0;
            });

            if (success)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("RegEmployee_DialogTSuccess", "RegEmployee_DialogDSuccess", AlertType.SUCCESS,
                        () =>
                        {
                            NavigationManager.Instance.GoBack();
                        });
                });
            }
        }

        private async Task EditEmployee()
        {
            byte[] profilePicData = GetProfilePicData();
            bool success = false;
            string validationErrorKey = null;
            string validationDescriptionKey = null;

            var updatedDto = new PersonalDTO
            {
                PersonalID = _editingEmployee.PersonalID,
                FirstName = TbEmployeeName.Text.Trim(),
                LastName = TbLastName.Text.Trim(),
                RFC = TbRFC.Text.Trim(),
                EmailAddress = TbEmailAddress.Text.Trim(),
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                ProfilePic = profilePicData,
                RoleID = _selectedRoleId,
                Address = new AddressDTO
                {
                    Id = _editingEmployee.AddressID,
                    AddressName = TbAddress.Text.Trim(),
                    ZipCode = TbZipCode.Text.Trim(),
                    City = TbCity.Text.Trim()
                }
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                if (!string.Equals(_editingEmployee.EmailAddress, updatedDto.EmailAddress, StringComparison.OrdinalIgnoreCase) &&
                    !client.IsPersonalEmailAvailable(updatedDto.EmailAddress))
                {
                    validationErrorKey = "GlbDialogD_EmailDuplicate";
                    validationDescriptionKey = "GlbDialogD_EmailDuplicate";
                    return;
                }

                if (!string.Equals(_editingEmployee.RFC, updatedDto.RFC, StringComparison.OrdinalIgnoreCase) &&
                    !client.IsRfcUnique(updatedDto.RFC))
                {
                    validationErrorKey = "RegEmployee_DialogTRfcDuplicate";
                    validationDescriptionKey = "RegEmployee_DialogDRfcDuplicate";
                    return;
                }

                success = await client.UpdatePersonalAsync(updatedDto);
            });

            if (!string.IsNullOrEmpty(validationErrorKey))
            {
                MessageDialog.Show(validationErrorKey, validationDescriptionKey, AlertType.WARNING);
                return;
            }

            if (success)
            {
                MessageDialog.Show("RegEmployee_DialogTEditSuccess", "RegEmployee_DialogDEditSuccess", AlertType.SUCCESS,
                    () => NavigationManager.Instance.GoBack());
            }
        }

        private bool IsEmailAvailable(string email)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return false; 

            return client.IsPersonalEmailAvailable(email);
        }

        private bool IsRfcUnique(string rfc)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return false;

            return client.IsRfcUnique(rfc);
        }

        private bool IsUsernameAvailable(string username)
        {
            var client = ServiceClientManager.Instance.Client;
            if (client == null) return false;

            return client.IsUsernameAvailable(username);
        }

        #region EventHandlers
        private async void Click_BtnRegisterEmployee(object sender, RoutedEventArgs e)
        {
            if (_selectedRoleId == 6 || PasswordUtilities.IsPasswordSecure(PbPassword.Password))
            {
                await RegisterEmployee();
            }
            else
            {
                MessageDialog.Show("RegEmployee_DialogTInvalidPassword", "RegEmployee_DialogDInvalidPassword", AlertType.WARNING);
            }
        }

        private async void Click_BtnEditEmployee(object sender, RoutedEventArgs e)
        {
            await EditEmployee();
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            SelectProfileImage(EmployeeProfilePic);
        }

        private void Click_BtnDeleteImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SetImageSource(EmployeeProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
            BtnDeleteImage.IsEnabled = false;
        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);
        }

        private void Password_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && PbPassword.Password != textBox.Text)
                PbPassword.Password = textBox.Text;
            else if (sender is PasswordBox passwordBox && TbPassword.Text != passwordBox.Password)
                TbPassword.Text = passwordBox.Password;

            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);
        }

        private void CbEmployeeRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbEmployeeRoles.SelectedItem is EmployeeRole selectedRole)
            {
                _selectedRoleId = selectedRole.Id;

                if (!_isEditMode) ToggleAccountVisibility(selectedRole.Id != 6);
                UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.ShowPassword(TbPassword, PbPassword);
            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.HidePassword(TbPassword, PbPassword);
            UpdateFormButtonState(_isEditMode ? BtnEditEmployee : BtnRegisterEmployee);
        }
        #endregion

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }
    }
}