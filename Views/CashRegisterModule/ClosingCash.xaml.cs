using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.CashRegisterModule
{
    public partial class ClosingCash : UserControl
    {
        private decimal _currentBalance = 0m;
        private bool _isUpdatingText = false;
        private CashRegisterPage _parentPage;

        public ClosingCash(CashRegisterPage parent)
        {
            InitializeComponent();
            TbCashBalance.Text = string.Empty;
            UpdateButtonState();
            ConfirmOpenCashCheckBox.Checked += (s, e) => UpdateButtonState();
            ConfirmOpenCashCheckBox.Unchecked += (s, e) => UpdateButtonState();
            TbDifference.TextChanged += (s, e) => UpdateButtonState();
            TbDifference.TextChanged += TbDifference_TextChanged;
            InputUtilities.ValidatePriceInput(TbDifference, @"^\d{0,4}(\.\d{0,2})?$", 99999.999m);
            LoadCashRegisterBalance();
            _parentPage = parent;
        }
        private async void LoadCashRegisterBalance()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var cashRegisterInfo = client?.GetOpenCashRegisterInfo();

                if (cashRegisterInfo != null)
                {
                    _currentBalance = cashRegisterInfo.CurrentBalance;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        TbCashBalance.Text = _currentBalance.ToString("C");
                    });
                }
            });
        }
        private void UpdateButtonState()
        {
            BtnAccept.IsEnabled = ConfirmOpenCashCheckBox.IsChecked == true &&
                                  !string.IsNullOrWhiteSpace(TbDifference.Text);
        }
        private async void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            if (!(ConfirmOpenCashCheckBox.IsChecked ?? false))
            {
                MessageDialog.Show("Advertencia", "Debe confirmar el cierre de caja.", AlertType.WARNING);
                return;
            }

            if (!decimal.TryParse(TbDifference.Text, System.Globalization.NumberStyles.Currency,
                                  System.Globalization.CultureInfo.CurrentCulture, out decimal cashierAmount))
            {
                MessageDialog.Show("Error", "Ingrese un monto válido en caja.", AlertType.ERROR);
                return;
            }

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                bool result = client.CloseCashRegister(cashierAmount);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (result)
                    {
                        MessageDialog.Show("Éxito", "Caja cerrada correctamente.", AlertType.SUCCESS, () =>
                        {
                            _parentPage?.LoadCurrentTransactionsData();
                            ClosePopup();
                        });
                    }
                    else
                    {
                        MessageDialog.Show("Error", "No hay caja abierta para cerrar.", AlertType.ERROR);
                    }
                });
            });
        }
        public static void Show(FrameworkElement triggerButton, CashRegisterPage parentPage)
        {
            var closingCash = new ClosingCash(parentPage);

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

            closingCash.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = closingCash.DesiredSize.Width;
            double popupHeight = closingCash.DesiredSize.Height;

            double left = activeWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            popUpHost.Content = closingCash;
            Canvas.SetLeft(popUpHost, left);
            Canvas.SetTop(popUpHost, top);

            popUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(closingCash, "ShowBorderAnimation");
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
        private void TbDifference_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText) return;

            if (decimal.TryParse(TbDifference.Text, System.Globalization.NumberStyles.Currency,
                                 System.Globalization.CultureInfo.CurrentCulture, out decimal userAmount))
            {
                decimal difference = userAmount - _currentBalance;

                _isUpdatingText = true;

                TxbDifferenceBalance.Text = difference.ToString("C");
                _isUpdatingText = false;
            }
            else
            {
                TxbDifferenceBalance.Text = string.Empty;
            }
        }
    }
}
