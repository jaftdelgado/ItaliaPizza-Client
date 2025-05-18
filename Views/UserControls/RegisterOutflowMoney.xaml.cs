using ItaliaPizzaClient.Utilities;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItaliaPizzaClient.Views.UserControls
{
    /// <summary>
    /// Lógica de interacción para RegisterOutflowMoney.xaml
    /// </summary>
    public partial class RegisterOutflowMoney : UserControl
    {
        public RegisterOutflowMoney()
        {
            InitializeComponent();
            InputUtilities.ValidatePriceInput(TbAmount);
            InputUtilities.ValidateInput(TbDescription,Constants.GENERAL_TEXT_PATTERN,Constants.MAX_LENGTH_DESCRIPTION);
        }
        public static void Show(FrameworkElement triggerButton)
        {
            var RegisterOutflowMoney = new RegisterOutflowMoney();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var popupContainer = mainWindow.PopUpHost.Parent as FrameworkElement;

            Point screenPos = triggerButton.PointToScreen(new Point(0, 0));
            Point containerPos = popupContainer.PointFromScreen(screenPos);

            RegisterOutflowMoney.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double popupWidth = RegisterOutflowMoney.DesiredSize.Width;
            double popupHeight = RegisterOutflowMoney.DesiredSize.Height;

            double left = mainWindow.ActualWidth - popupWidth - 20;
            if (left < 0) left = 0;

            double top = containerPos.Y + triggerButton.ActualHeight + 14;

            mainWindow.PopUpHost.Content = RegisterOutflowMoney;
            Canvas.SetLeft(mainWindow.PopUpHost, left);
            Canvas.SetTop(mainWindow.PopUpHost, top);

            mainWindow.PopUpOverlay.Visibility = Visibility.Visible;

            Animations.BeginAnimation(RegisterOutflowMoney, "ShowBorderAnimation");
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
                System.Globalization.NumberStyles.Currency,
                System.Globalization.CultureInfo.CurrentCulture
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
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.PopUpOverlay.Visibility = Visibility.Collapsed;
            mainWindow.PopUpHost.Content = null;
        }

        private void TbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
