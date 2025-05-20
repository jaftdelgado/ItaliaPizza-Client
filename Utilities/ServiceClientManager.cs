using ItaliaPizzaClient.ItaliaPizzaServices;
using ItaliaPizzaClient.Views.Dialogs;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Utilities
{
    public class ServiceClientManager
    {
        private static readonly Lazy<ServiceClientManager> _instance =
            new Lazy<ServiceClientManager>(() => new ServiceClientManager());

        private readonly object _lock = new object();
        private ChannelFactory<IMainManager> _channelFactory;
        private IMainManager _serviceClient;
        private DateTime _lastPingTime = DateTime.MinValue;
        private const int PingIntervalMinutes = 5;

        public static ServiceClientManager Instance => _instance.Value;

        private ServiceClientManager()
        {
            CreateClient();
        }

        public IMainManager Client
        {
            get
            {
                lock (_lock)
                {
                    if (_serviceClient == null || ((ICommunicationObject)_serviceClient).State == CommunicationState.Faulted)
                    {
                        var client = TryReconnect();
                        if (client == null)
                            throw new CommunicationException("GlbDialogD_NoConnection");

                        return client;
                    }

                    if ((DateTime.Now - _lastPingTime).TotalMinutes > PingIntervalMinutes)
                        Task.Run(() => CheckConnectionAsync());

                    return _serviceClient;
                }
            }
        }

        private async Task CheckConnectionAsync()
        {
            try
            {
                await Task.Run(() => _serviceClient?.Ping());
                _lastPingTime = DateTime.Now;
            }
            catch
            {
                lock (_lock) CloseClient();
            }
        }

        private void CreateClient()
        {
            lock (_lock)
            {
                try
                {
                    _channelFactory?.Abort();
                    _channelFactory = new ChannelFactory<IMainManager>("*");
                    _serviceClient = _channelFactory.CreateChannel();

                    var commObj = (ICommunicationObject)_serviceClient;
                    if (commObj.State != CommunicationState.Opened)
                    {
                        commObj.Open();
                        _lastPingTime = DateTime.Now;
                    }
                }
                catch
                {
                    _serviceClient = null;
                }
            }
        }

        private IMainManager TryReconnect()
        {
            lock (_lock)
            {
                try
                {
                    CreateClient();
                    _serviceClient?.Ping();
                    _lastPingTime = DateTime.Now;
                    return _serviceClient;
                }
                catch (TimeoutException)
                {
                    throw new TimeoutException("GlbDialogD_TimeOut");
                }
                catch (Exception)
                {
                    throw new CommunicationException("GlbDialogD_NoConnection");
                }
            }
        }

        public void CloseClient()
        {
            lock (_lock)
            {
                if (_serviceClient == null)
                    return;

                var commObj = _serviceClient as ICommunicationObject;
                if (commObj != null)
                {
                    try
                    {
                        if (commObj.State == CommunicationState.Opened)
                            commObj.Close();
                        else if (commObj.State == CommunicationState.Faulted)
                            commObj.Abort();
                    }
                    catch
                    {
                        commObj.Abort();
                    }
                }

                _serviceClient = null;
            }
        }

        public static async Task ExecuteServerAction(Func<Task> action, bool showLoading = true)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => LoadingDialog.Show());

            try
            {
                await Task.Run(action);
            }
            catch (TimeoutException ex) when (ex.Message == "GlbDialogD_TimeOut")
            {
                await ShowErrorDialogAsync("GlbDialogT_TimeOut", ex.Message);
            }
            catch (CommunicationException ex) when (ex.Message == "GlbDialogD_NoConnection")
            {
                await ShowErrorDialogAsync("GlbDialogT_NoConnection", ex.Message);
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Excepción", $"Ocurrió una excepción: {ex.Message}\n{ex.StackTrace}");
                Console.WriteLine($"Ocurrió una excepción: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (showLoading)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (LoadingDialog.IsDialogVisible())
                            LoadingDialog.Close();
                    });
                }
            }
        }

        private static async Task ShowErrorDialogAsync(string titleKey, string messageKey)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                LoadingDialog.Close();
                MessageDialog.Show(titleKey, messageKey, AlertType.ERROR);
            });
        }
    }
}