using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Shared.API.Utilities;

namespace Piipan.QueryTool.Client.Helpers
{
    public static class ValueToDisplayHelper
    {
        public const string DateFormat = "MM/dd/yyyy";
        public static string GetDisplayValue<T>(T value)
        {
            if (value == null)
            {
                return "-";
            }
            return value switch
            {
                bool b => b ? "Yes" : "No",
                DateTime d => d.ToString(DateFormat),
                DateRange d => DateRangeFormat(d),
                IEnumerable<DateRange> ds => ds?.Count() == 0 ? "-" : string.Join('\n', ds.Select(d => DateRangeFormat(d))),
                _ => value.ToString()
            };
        }
        private static string DateRangeFormat(DateRange dateRange)
        {
            return $"{dateRange.Start.ToString(DateFormat)} - {dateRange.End.ToString(DateFormat)}";
        }
    }
}
