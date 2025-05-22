using ItaliaPizzaClient.Model;
using System;

namespace ItaliaPizzaClient.Utilities
{
    public static class CurrentSession
    {
        public static Personal LoggedInUser { get; private set; }
        public static DateTime StartTime { get; set; }
        public static bool IsActive => LoggedInUser != null;

        public static void SetUser(Personal user)
        {
            LoggedInUser = user;
            StartTime = DateTime.Now;
        }

        public static void LogOut()
        {
            LoggedInUser = null;
            StartTime = DateTime.MinValue;
        }
    }
}
