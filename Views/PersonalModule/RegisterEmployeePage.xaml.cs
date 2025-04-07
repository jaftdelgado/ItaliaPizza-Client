﻿using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            InputUtilities.ValidateInput(TbRFC, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_RFC);
            InputUtilities.ValidateInput(TbEmployeeName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbFatherName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbMotherName, Constants.NAMES_PATTERN, Constants.MAX_LENGTH_NAMES);
            InputUtilities.ValidateInput(TbUsername, Constants.ALPHANUMERIC_PATTERN, Constants.MAX_LENGTH_USERNAME);
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
            var requiredFields = new List<object> { TbRFC, TbEmployeeName, TbFatherName, TbMotherName };

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

        public void RegisterEmployee()
        {
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            var personalDto = new PersonalDTO
            {
                FirstName = TbEmployeeName.Text.Trim(),
                FatherName = TbFatherName.Text.Trim(),
                MotherName = TbMotherName.Text.Trim(),
                RFC = TbRFC.Text.Trim(),
                Username = string.IsNullOrWhiteSpace(TbUsername.Text) ? null : TbUsername.Text.Trim(),
                Password = (_selectedRoleId != 6 && !string.IsNullOrWhiteSpace(PbPassword.Password))
                    ? PasswordUtilities.HashPassword(PbPassword.Password)
                    : null,
                ProfilePic = ImageUtilities.ImageToByteArray((BitmapImage)EmployeeProfilePic.Source),
                RoleID = _selectedRoleId
            };

            if (_selectedRoleId != 6 && IsUsernameTaken(personalDto.Username))
            {
                MessageDialog.Show("RegEmployee_DialogTUserDuplicate", "RegEmployee_DialogDUserDuplicate", AlertType.WARNING);
                return;
            }

            if (ValidateRFC(personalDto.RFC))
            {
                MessageDialog.Show("RegEmployee_DialogTRfcDuplicate", "RegEmployee_DialogDRfcDuplicate", AlertType.WARNING);
                return;
            }

            ConnectionUtilities.ExecuteDatabaseSafeAction(() =>
            {
                int result = client.AddPersonal(personalDto);
                if (result > 0)
                {
                    MessageDialog.Show("RegEmployee_DialogTSuccess", "RegEmployee_DialogDSuccess", AlertType.SUCCESS);
                }
            });

            ConnectionUtilities.CloseClient();
        }

        private bool IsUsernameTaken(string username)
        {
            return ConnectionUtilities.ExecuteDatabaseSafeFunction(() =>
            {
                var service = ConnectionUtilities.IsServerConnected();
                if (service == null) return false;
                return service.IsUsernameAvailable(username);
            }, false);
        }

        private bool ValidateRFC(string rfc)
        {
            return ConnectionUtilities.ExecuteDatabaseSafeFunction(() =>
            {
                var service = ConnectionUtilities.IsServerConnected();
                if (service == null) return false;
                return service.IsRfcUnique(rfc);
            }, false);
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
            var client = ConnectionUtilities.IsServerConnected();
            if (client == null) return;

            MessageBox.Show("Conexión exitosa con el servidor.", "Ping OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}