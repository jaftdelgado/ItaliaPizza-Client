using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.UserControls
{
    public partial class OpeningCash : UserControl
    {
        public OpeningCash()
        {
            InitializeComponent();
            InitializeOpeningDate();
            UpdateButtonState();
            ConfirmOpenCashCheckBox.Checked += (s, e) => UpdateButtonState();
            ConfirmOpenCashCheckBox.Unchecked += (s, e) => UpdateButtonState();
            TbInitialBalance.TextChanged += (s, e) => UpdateButtonState();
            InputUtilities.ValidatePriceInput(TbInitialBalance, @"^\d{0,4}(\.\d{0,2})?$", 99999.999m);
        }

        private void InitializeOpeningDate()
        {
            TxbDateOpening.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void UpdateButtonState()
        {
            BtnAccept.IsEnabled = ConfirmOpenCashCheckBox.IsChecked == true &&
                                !string.IsNullOrWhiteSpace(TbInitialBalance.Text);
        }

        private async void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            await OpenCashRegister();
        }

        private async Task OpenCashRegister()
        {
            decimal initialBalance = decimal.Parse(TbInitialBalance.Text, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture);
            var client = ServiceClientManager.Instance.Client;
            if (client == null)
            {
                MessageDialog.Show("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
                return;
            }

            bool success = false;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                success = await client.OpenCashRegisterAsync(initialBalance);
            });

            HandleOpenCashResult(success);
        }

        private void HandleOpenCashResult(bool success)
        {
            if (success)
                MessageDialog.Show("CashRegister_DialogTSuccess", "CashRegister_DialogDSuccess", AlertType.SUCCESS, ClosePopup);

            else
                MessageDialog.Show("CashRegister_DialogTFail", "CashRegister_DialogDAlreadyOpen", AlertType.WARNING);
        }

        public static void Show(FrameworkElement triggerButton)
        {
            var openingCash = new OpeningCash();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var popupContainer = mainWindow.PopUpHost.Parent as FrameworkElement;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            openingCash.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = openingCash.DesiredSize.Width;
            double popupHeight = openingCash.DesiredSize.Height;

            double left = mainWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            mainWindow.PopUpHost.Content = openingCash;
            Canvas.SetLeft(mainWindow.PopUpHost, left);
            Canvas.SetTop(mainWindow.PopUpHost, top);

            mainWindow.PopUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(openingCash, "ShowBorderAnimation");
        }

        private void ClosePopup()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.PopUpOverlay.Visibility = Visibility.Collapsed;
            mainWindow.PopUpHost.Content = null;
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }
    }
}