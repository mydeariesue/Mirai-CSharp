using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Epic_Bot.Helper
{
    public class AppSettingHelper
    {
        public static string GetSettingValue(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
        public static int GetSettingValueToInt(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrEmpty(value)) return 0;
            int.TryParse(value, out int intValue);
            return intValue;
        }

        public static float GetSettingValueToFloat(string key)
        {
            var value = ConfigurationManager.AppSettings.Get(key);
            if (string.IsNullOrEmpty(value)) return 0;
            float.TryParse(value, out float floatValue);
            return floatValue;
        }
    }
}
