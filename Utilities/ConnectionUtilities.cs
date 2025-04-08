using System;
using System.ServiceModel;
using ItaliaPizzaClient.Views.Dialogs;
using ItaliaPizzaClient.ItaliaPizzaServices;
using System.Data.SqlClient;

namespace ItaliaPizzaClient.Utilities
{
    public static class ConnectionUtilities
    {
        private static ChannelFactory<IMainManager> _channelFactory;
        private static IMainManager _service;

        // Manejo de la conexión con el servidor
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
                    MessageDialog.Show("GlbDialogT_NoConnection", "GlbDialogD_NoConnection", AlertType.ERROR);
                    return null;
                }
            }
            catch (TimeoutException)
            {
                MessageDialog.Show("GlbDialogT_TimeOut", "GlbDialogD_TimeOut", AlertType.ERROR);
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

        public static void ExecuteDatabaseSafeAction(Action action)
        {
            try
            {
                action();
            }
            catch (SqlException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
            }
            catch (InvalidOperationException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
            }
            catch (FaultException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
            }
            catch (Exception)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
            }
        }

        public static T ExecuteDatabaseSafeFunction<T>(Func<T> func, T defaultValue = default)
        {
            try
            {
                return func();
            }
            catch (SqlException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                return defaultValue;
            }
            catch (InvalidOperationException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                return defaultValue;
            }
            catch (FaultException)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                return defaultValue;
            }
            catch (Exception)
            {
                MessageDialog.Show("GlbDialogT_DBNoConnection", "GlbDialogD_DBNoConnection", AlertType.ERROR);
                return defaultValue;
            }
        }
    }
}
