using System;
using System.Text;

namespace gpconnect_analytics.Helpers
{
    public static class StringExtensions
    {
        public static string StringToBase64(this string valueIn)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(valueIn));
        }
    }
}
