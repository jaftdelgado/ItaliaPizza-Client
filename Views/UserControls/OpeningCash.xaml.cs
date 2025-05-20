using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Linq;
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
            if (!decimal.TryParse(TbInitialBalance.Text, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture, out decimal initialBalance))
            {
                MessageDialog.Show("GlbDialogT_InvalidInput", "GlbDialogD_InvalidBalance", AlertType.WARNING);
                return;
            }

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

            var activeWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow == null) return;

            var popUpHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popUpOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;
            var popupContainer = popUpHost?.Parent as FrameworkElement;

            if (popUpHost == null || popUpOverlay == null || popupContainer == null)
            {
                MessageBox.Show("No se pudo encontrar el contenedor del popup.");
                return;
            }

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            openingCash.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = openingCash.DesiredSize.Width;
            double popupHeight = openingCash.DesiredSize.Height;

            double left = activeWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            popUpHost.Content = openingCash;
            Canvas.SetLeft(popUpHost, left);
            Canvas.SetTop(popUpHost, top);

            popUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(openingCash, "ShowBorderAnimation");
        }

        private void ClosePopup()
        {
            var activeWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (activeWindow == null) return;

            var popUpHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popUpOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;

            if (popUpHost != null) popUpHost.Content = null;
            if (popUpOverlay != null) popUpOverlay.Visibility = Visibility.Collapsed;
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }
    }
}
