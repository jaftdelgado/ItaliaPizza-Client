using ItaliaPizzaClient.ItaliaPizzaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ItaliaPizzaClient.Utilities
{
    public static class SessionManager
    {
        private static DispatcherTimer _timer;
        public static void Start()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };

            _timer.Tick += (s, e) =>
            {
                if (CurrentSession.IsActive)
                {
                    MainManagerClient service = new MainManagerClient();
                    //service.UpdateActivity(CurrentSession.UserID);
                }
            };

            _timer.Start();
        }
        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }
    }
}
