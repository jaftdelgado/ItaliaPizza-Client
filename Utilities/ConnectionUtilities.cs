using System;
using System.ServiceModel;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.ItaliaPizzaServices;
using System.Data.SqlClient;
using System.Windows;
using System.Threading.Tasks;
using ItaliaPizzaClient.Views;

namespace ItaliaPizzaClient.Utilities
{
    public static class ConnectionUtilities
    {
        private static ChannelFactory<IMainManager> _channelFactory;
        private static IMainManager _service;

        public static IMainManager IsServerConnected()
        {
            try
            {
                EnsureClientConnected();
                _service.Ping();
                return _service;
            }
            catch (CommunicationException)
            {
                try
                {
                    ResetClient();
                    _service.Ping();
                    return _service;
                }
                catch
                {
                    ShowSafeDialog("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
                    return null;
                }
            }
            catch (TimeoutException)
            {
                ShowSafeDialog("GlbDialogT_TimeOut", "GlbDialogD_TimeOut", AlertType.ERROR);
                return null;
            }
        }

        private static void EnsureClientConnected()
        {
            if (_service == null || ((ICommunicationObject)_service).State == CommunicationState.Faulted)
                ResetClient();

            var commObj = (ICommunicationObject)_service;

            if (commObj.State != CommunicationState.Opened)
                commObj.Open();
        }

        private static void ResetClient()
        {
            _channelFactory?.Abort();
            _channelFactory = new ChannelFactory<IMainManager>("*");
            _service = _channelFactory.CreateChannel();
        }

        public static void CloseClient()
        {
            if (_service == null) return;

            var commObj = _service as ICommunicationObject;
            if (commObj != null)
            {
                if (commObj.State == CommunicationState.Opened)
                    commObj.Close();
                else if (commObj.State == CommunicationState.Faulted)
                    commObj.Abort();
            }

            _service = null;
        }

        public static void ShowSafeDialog(string titleKey, string descriptionKey, AlertType alertType)
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

    }
}
