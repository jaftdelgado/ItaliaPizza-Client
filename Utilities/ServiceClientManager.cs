using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.Views;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace ItaliaPizzaClient.Utilities
{
    public class ServiceClientManager
    {
        private static readonly Lazy<ServiceClientManager> _instance =
            new Lazy<ServiceClientManager>(() => new ServiceClientManager());

        private ChannelFactory<IMainManager> _channelFactory;
        private IMainManager _serviceClient;

        public static ServiceClientManager Instance => _instance.Value;

        private ServiceClientManager()
        {
            CreateClient();
        }

        public IMainManager Client
        {
            get
            {
                if (_serviceClient == null || ((ICommunicationObject)_serviceClient).State == CommunicationState.Faulted)
                    return TryReconnect();

                return _serviceClient;
            }
        }

        private void CreateClient()
        {
            try
            {
                _channelFactory?.Abort();
                _channelFactory = new ChannelFactory<IMainManager>("*");
                _serviceClient = _channelFactory.CreateChannel();

                var commObj = (ICommunicationObject)_serviceClient;
                if (commObj.State != CommunicationState.Opened)
                    commObj.Open();
            }
            catch
            {
                _serviceClient = null;
            }
        }

        private IMainManager TryReconnect()
        {
            try
            {
                CreateClient();
                _serviceClient?.Ping();
                return _serviceClient;
            }
            catch (TimeoutException)
            {
                ShowSafeDialog("GlbDialogT_TimeOut", "GlbDialogD_TimeOut", AlertType.ERROR);
            }
            catch (Exception)
            {
                ShowSafeDialog("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
            }

            return null;
        }

        public void CloseClient()
        {
            if (_serviceClient == null)
                return;

            var commObj = _serviceClient as ICommunicationObject;
            if (commObj != null)
            {
                if (commObj.State == CommunicationState.Opened)
                    commObj.Close();
                else if (commObj.State == CommunicationState.Faulted)
                    commObj.Abort();
            }

            _serviceClient = null;
        }

        public static async Task ExecuteServerAction(Func<Task> action)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => LoadingDialog.Show());

            try
            {
                await Task.Run(action);
            }
            catch (Exception)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LoadingDialog.Close();
                    MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (IsLoadingDialogVisible())
                        LoadingDialog.Close();
                });
            }
        }

        private static bool IsLoadingDialogVisible()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            return mainWindow?.DialogHost.Content is LoadingDialog;
        }

        private static void ShowSafeDialog(string titleKey, string descriptionKey, AlertType alertType)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                MessageDialog.Show(titleKey, descriptionKey, alertType);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageDialog.Show(titleKey, descriptionKey, alertType);
                });
            }
        }
    }
}
