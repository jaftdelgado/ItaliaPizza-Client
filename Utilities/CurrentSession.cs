using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItaliaPizzaClient.Utilities
{
    public static class CurrentSession
    {
        public static int UserID { get; set; } = 0;
        public static int UserRole { get; set; } = 0;
        public static string Name { get; set; } = string.Empty;
        public static string Surnames { get; set; } = string.Empty;
        public static string UserName { get; set; } = string.Empty;
        public static string UserId { get; set; } = string.Empty;
        public static DateTime StartTime { get; set; }
        public static bool IsActive => UserID > 0;

        public static void Logout()
        {
            UserID = 0;
            UserRole = 0;
            Name = string.Empty;
            Surnames = string.Empty;
            UserName = string.Empty;
            UserId = string.Empty;
            StartTime = DateTime.MinValue;
        }
    }
}
