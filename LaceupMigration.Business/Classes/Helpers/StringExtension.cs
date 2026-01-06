





using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public static class StringExtension
    {
        public static string ToCustomString(this decimal value)
        {
            if (!Config.DontRoundInUI)
                return value.ToString("C", CultureInfo.CurrentCulture);

            return value.ToString($"N{Config.Round}").Replace(",", "");
        }

        public static string ToCustomString(this float value)
        {
            if (!Config.DontRoundInUI)
                return value.ToString("C", CultureInfo.CurrentCulture);

            return value.ToString($"N{Config.Round}").Replace(",", "");
        }

        public static string ToCustomString(this double value)
        {
            if (!Config.DontRoundInUI)
                return value.ToString("C", CultureInfo.CurrentCulture);

            return value.ToString($"N{Config.Round}").Replace(",", "");
        }

        public static string ToCustomString(this int value)
        {
            if (!Config.DontRoundInUI)
                return value.ToString("C", CultureInfo.CurrentCulture);

            return value.ToString($"N{Config.Round}").Replace(",", "");
        }
    }
}