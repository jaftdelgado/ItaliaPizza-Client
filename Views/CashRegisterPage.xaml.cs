using ItaliaPizzaClient.Model;
using ItaliaPizzaClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ItaliaPizzaClient.Views
{
    public partial class CashRegisterPage : Page
    {
        private List<Transaction> _allTransactions = new List<Transaction>();
        private bool _hasOpenCashRegister = false;

        public CashRegisterPage()
        {
            InitializeComponent();
            Loaded += CashRegisterPage_Loaded;
        }

        private void CashRegisterPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentTransactionsData();
        }

        private async void LoadCurrentTransactionsData()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                _hasOpenCashRegister = client.HasOpenCashRegister();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    BtnOpenRegister.Visibility = _hasOpenCashRegister ? Visibility.Collapsed : Visibility.Visible;
                    BtnCloseRegister.Visibility = _hasOpenCashRegister ? Visibility.Visible : Visibility.Collapsed;
                    TbBalance.Visibility = _hasOpenCashRegister ? Visibility.Visible : Visibility.Collapsed;
                });

                if (!_hasOpenCashRegister) return;

                var cashRegisterInfo = client.GetOpenCashRegisterInfo();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cashRegisterInfo != null)
                        TbBalance.Text = cashRegisterInfo.CurrentBalance.ToString("C");
                    else
                    {
                        TbBalance.Text = string.Empty;
                    }
                });

                var dtoList = client.GetCurrentTransactions();

                var transactions = dtoList.Select(dto => new Transaction
                {
                    TransactionID = dto.TransactionID,
                    CashRegisterID = dto.CashRegisterID,
                    FinancialFlow = dto.FinancialFlow,
                    Amount = dto.Amount ?? 0m,
                    Date = dto.Date,
                    Concept = dto.Concept,
                    Description = dto.Description,
                    OrderID = dto.OrderID,
                    SupplierOrderID = dto.SupplierOrderID,
                }).ToList();

                _allTransactions = transactions;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    TransactionsDataGrid.ItemsSource = _allTransactions;
                });
            });
        }

        private void DisplayTransactionDetails(Transaction selected)
        {
            if (selected == null) return;

            UpdateTransactionPanelVisibility(selected);

            TransactionConcept.Text = selected.ConceptDescription;
            TransactionDescription.Text = selected.Description;
        }

        private void TransactionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selected)
                DisplayTransactionDetails(selected);
            else
                UpdateTransactionPanelVisibility(null);
        }

        private void UpdateTransactionPanelVisibility(Transaction selected)
        {
            bool hasSelection = selected != null;
            TransactionDetailsPanel.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Click_BtnOpenRegister(object sender, RoutedEventArgs e)
        {
            UserControls.OpeningCash.Show(sender as FrameworkElement);
        }

        private void Click_BtnRegisterOutflow(object sender, RoutedEventArgs e)
        {
            UserControls.RegisterOutflowMoney.Show(sender as FrameworkElement);
        }

        private void Click_BtnCloseRegister(object sender, RoutedEventArgs e)
        {

        }
    }
}
