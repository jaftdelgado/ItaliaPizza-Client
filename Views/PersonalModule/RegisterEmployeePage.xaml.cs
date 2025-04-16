using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ItaliaPizzaClient.Views
{
    public partial class RegisterEmployeePage : Page
    {
        private bool _isAccountVisible = false;
        private int _selectedRoleId = -1;

        public RegisterEmployeePage()
        {
            InitializeComponent();
            SetRolesComboBox();
            SetInputFields();
            UpdateRegisterButtonState();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            NavigationManager.Initialize(mainWindow.MainFrame, mainWindow.NavigationPanel, mainWindow.BtnBack);

            ImageUtilities.SetImageSource(EmployeeProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
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
            InputUtilities.ValidateInput(TbEmployeeName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbLastName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbPhoneNumber, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_PHONENUMBER);
            InputUtilities.ValidateInput(TbEmailAddress, Constants.EMAIL_ALLOWED_CHARS_PATTERN, Constants.MAX_LENGTH_EMAIL);
            InputUtilities.ValidateInput(TbAddress, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_ADDRESSNAME);
            InputUtilities.ValidateInput(TbZipCode, Constants.NUMERIC_PATTERN, Constants.MAX_LENGTH_ZIPCODE);
            InputUtilities.ValidateInput(TbCity, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_CITY);
            InputUtilities.ValidateInput(TbRFC, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_RFC);
            InputUtilities.ValidateInput(TbUsername, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_USERNAME);
            InputUtilities.ValidateInput(TbPassword, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_PASSWORD);
            InputUtilities.ValidatePasswordInput(PbPassword, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_PASSWORD);

            InputUtilities.ConvertToUpperCase(TbRFC);
            InputUtilities.ConvertToLowerCase(TbEmailAddress);
            InputUtilities.ConvertToLowerCase(TbUsername);
        }

        private void SelectProfileImage(Image targetImageControl, int targetWidth, int targetHeight)
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
                    if (!ImageUtilities.IsImageSizeValid(openFileDialog.FileName, Constants.MAX_IMAGE_SIZE))
                    {
                        string errorMessage = string.Format(Application.Current.Resources["GlbDialogD_InvalidImageSize"].ToString(),
                            Application.Current.Resources["Glb_MaxImageSizeMB"].ToString());

                        MessageDialog.Show("GlbDialogT_InvalidImageSize", errorMessage, AlertType.WARNING);
                        return;
                    }

                    var resizedImage = ImageUtilities.LoadAndResizeImage(openFileDialog.FileName, targetWidth, targetHeight);
                    targetImageControl.Source = resizedImage;
                    BtnDeleteImage.IsEnabled = true;
                }
                catch (IOException)
                {
                    MessageDialog.Show("GlbDialogT_InvalidImageSize", "GlbDialogD_InvalidImageSize", AlertType.WARNING);
                }
            }
        }

        private void UpdateRegisterButtonState()
        {
            var requiredFields = new List<object> { TbRFC, TbEmployeeName };

            if (_selectedRoleId != 6)
            {
                requiredFields.Add(TbUsername);
                requiredFields.Add(TbPassword);
            }

            bool allFieldsFilled = true;
            foreach (TextBox field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    allFieldsFilled = false;
                    break;
                }
            }

            BtnRegisterEmployee.IsEnabled = allFieldsFilled;
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

                if (ImageUtilities.AreImageEqual(profilePicData, defaultImageBytes))
                {
                    profilePicData = null;
                }
            }

            return profilePicData;
        }

        public void RegisterEmployee()
        {
            var client = ConnectionUtilities.IsServerConnected();
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

            ConnectionUtilities.ExecuteDatabaseSafeAction(() =>
            {
                if (_selectedRoleId != 6 && !IsUsernameAvailable(personalDto.Username)) return;
                if (!IsEmailAvailable(personalDto.EmailAddress)) return;
                if (!IsRfcUnique(personalDto.RFC)) return;

                int result = client.AddPersonal(personalDto);
                if (result > 0)
                {
                    MessageDialog.Show("RegEmployee_DialogTSuccess", "RegEmployee_DialogDSuccess", AlertType.SUCCESS);
                    NavigationManager.Instance.GoBack();
                }
            });
        }

        private bool IsUsernameAvailable(string username)
        {
            var service = ConnectionUtilities.IsServerConnected();
            if (service == null) return false;

            bool isUsernameAvailable = service.IsUsernameAvailable(username);
            if (!isUsernameAvailable)
                MessageDialog.Show("RegEmployee_DialogTUserDuplicate", "RegEmployee_DialogDUserDuplicate", AlertType.WARNING);

            return isUsernameAvailable;
        }
        
        private bool IsEmailAvailable(string email)
        {
            var service = ConnectionUtilities.IsServerConnected();
            if (service == null) return false;

            bool isEmailAvailable = service.IsPersonalEmailAvailable(email);
            if (!isEmailAvailable)
                MessageDialog.Show("GlbDialogD_EmailDuplicate", "GlbDialogD_EmailDuplicate", AlertType.WARNING);

            return isEmailAvailable;
        }

        private bool IsRfcUnique(string rfc)
        {
            var service = ConnectionUtilities.IsServerConnected();
            if (service == null) return false;

            bool isRfcUnique = service.IsRfcUnique(rfc);
            if (!isRfcUnique)
                MessageDialog.Show("RegEmployee_DialogTRfcDuplicate", "RegEmployee_DialogDRfcDuplicate", AlertType.WARNING);

            return isRfcUnique;
        }

        #region EventHandlers
        private void Click_BtnRegisterEmployee(object sender, RoutedEventArgs e)
        {
            if (_selectedRoleId == 6 || PasswordUtilities.IsPasswordSecure(PbPassword.Password))
            {
                RegisterEmployee();
            }
            else
                MessageDialog.Show("RegEmployee_DialogTInvalidPassword", "RegEmployee_DialogDInvalidPassword", AlertType.WARNING);
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            SelectProfileImage(EmployeeProfilePic, 180, 180);
        }

        private void Click_BtnDeleteImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SetImageSource(EmployeeProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
            BtnDeleteImage.IsEnabled = false;
        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateRegisterButtonState();
        }

        private void Password_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && PbPassword.Password != textBox.Text)
                PbPassword.Password = textBox.Text;
            else if (sender is PasswordBox passwordBox && TbPassword.Text != passwordBox.Password)
                TbPassword.Text = passwordBox.Password;

            UpdateRegisterButtonState();
        }

        private void CbEmployeeRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbEmployeeRoles.SelectedItem is EmployeeRole selectedRole)
            {
                _selectedRoleId = selectedRole.Id;
                ToggleAccountVisibility(selectedRole.Id != 6);
                UpdateRegisterButtonState();
            }
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.ShowPassword(TbPassword, PbPassword);
            UpdateRegisterButtonState();
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.HidePassword(TbPassword, PbPassword);
            UpdateRegisterButtonState();
        }
        #endregion

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Instance.GoBack();
        }
    }
}
