using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.UserControls
{
    /// <summary>
    /// Lógica de interacción para RegisterOutflowMoney.xaml
    /// </summary>
    public partial class RegisterOutflowMoney : UserControl
    {
        private CashRegisterPage _parentPage;
        public RegisterOutflowMoney(CashRegisterPage parent)
        {
            InitializeComponent();
            _parentPage = parent;
            InputUtilities.ValidatePriceInput(TbAmount);
            InputUtilities.ValidateInput(TbDescription, Constants.GENERAL_TEXT_PATTERN, Constants.MAX_LENGTH_DESCRIPTION);
        }

        public static void Show(FrameworkElement triggerButton, CashRegisterPage parentPage)
        {
            var control = new RegisterOutflowMoney(parentPage);

            var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (activeWindow == null)
                return;

            var popupHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popupOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;

            if (popupHost == null || popupOverlay == null)
                return;

            var popupContainer = popupHost.Parent as FrameworkElement;
            if (popupContainer == null) return;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = control.DesiredSize.Width;
            double left = activeWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            popupHost.Content = control;
            Canvas.SetLeft(popupHost, left);
            Canvas.SetTop(popupHost, top);
            popupOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(control, "ShowBorderAnimation");
        }

        private async void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(TbAmount.Text))
            {
                Animations.ShakeTextBox(TbAmount);
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(TbDescription.Text))
            {
                Animations.ShakeTextBox(TbDescription);
                hasError = true;
            }

            if (hasError)
                return;

            decimal amount = decimal.Parse(
                TbAmount.Text,
                NumberStyles.Currency,
                CultureInfo.CurrentCulture
            );
            string description = TbDescription.Text.Trim();

            var client = ServiceClientManager.Instance.Client;
            if (client == null)
            {
                MessageDialog.Show("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
                return;
            }

            int result = 0;
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                result = await client.RegisterCashOutAsync(amount, description);
            });

            switch (result)
            {
                case 1:
                    MessageDialog.Show("OutflowMoney_DialogTSuccess", "OutflowMoney_DialogDSuccess", AlertType.SUCCESS, () =>
                    {
                        _parentPage?.LoadCurrentTransactionsData();
                        ClosePopup();
                    });
                    break;

                case -1:
                    MessageDialog.Show("OutflowMoney_DialogTNoCashOpen", "OutflowMoney_DialogDNoCashOpen", AlertType.WARNING);
                    break;

                case -2:
                    MessageDialog.Show("OutflowMoney_DialogTInsufficientFunds", "OutflowMoney_DialogDInsufficientFunds", AlertType.WARNING);
                    break;

                default:
                    MessageDialog.Show("GlbDialogT_Error", "GlbDialogD_GenericError", AlertType.ERROR);
                    break;
            }
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }

        private void ClosePopup()
        {
            var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (activeWindow == null)
                return;

            var popupHost = activeWindow.FindName("PopUpHost") as ContentControl;
            var popupOverlay = activeWindow.FindName("PopUpOverlay") as UIElement;

            if (popupHost == null || popupOverlay == null)
                return;

            if (popupHost.Content == this)
            {
                popupHost.Content = null;
                popupOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void TbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
