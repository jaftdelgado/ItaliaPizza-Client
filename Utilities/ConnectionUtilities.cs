using System;
using System.ServiceModel;
using ItaliaPizzaClient.Views.Dialogs;

using ItaliaPizzaClient.ServiceReference;

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
    }
}
