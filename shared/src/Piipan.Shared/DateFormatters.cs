using System;
using System.Collections.Generic;

namespace Piipan.Shared.Helpers
{
    public class DateFormatters
    {
        /// <summary>
        /// Format a list of DateTimes as a Ppstgresql Array of Dates
        /// </summary>
        public static string FormatDatesAsPgArray(List<DateTime> dates) {
            List<string> formattedDateStrings = new List<string>();
            string formatted = "{";
            dates.Sort((x, y) => y.CompareTo(x));
            foreach (var date in dates)
            {
                formattedDateStrings.Add(date.ToString("yyyy-MM-dd"));
            }
            formatted += string.Join(",", formattedDateStrings);
            formatted += "}";
            return formatted;
        }
    }
}
