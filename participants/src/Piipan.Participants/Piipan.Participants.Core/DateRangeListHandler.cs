using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using NpgsqlTypes;
using Piipan.Shared.Utilities;

namespace Piipan.Participants.Core
{
    /// <summary>
    /// Custom handlers for converting sql columns daterange[] to C# properties
    /// Converts sql arrays of datetime ranges into C# Lists of DateRange
    /// </summary>
    /// <remarks>
    /// Used when configuring Dapper as SqlMapper.AddTypeHandler(new DateRangeListHandler());
    /// </remarks>
    public class DateRangeListHandler : SqlMapper.TypeHandler<IEnumerable<DateRange>>
    {
        // <summary>
        // Implement custom parsing to match PostgreSQL handling of bound inclusion.
        // See: https://www.postgresql.org/docs/current/rangetypes.html#RANGETYPES-INCLUSIVITY
        // </summary>
        public override IEnumerable<DateRange> Parse(object value)
        {
            if (value is DBNull)
            {
                return new List<DateRange>();
            }

            IEnumerable<DateRange> typedValue = ((NpgsqlRange<DateTime>[])value)
                .Select(range => new DateRange()
                {
                    Start = range.LowerBoundIsInclusive ? range.LowerBound : range.LowerBound.AddDays(1),
                    End = range.UpperBoundIsInclusive ? range.UpperBound : range.UpperBound.AddDays(-1)
                })
                .ToList();

            return typedValue;
        }

        // <summary>
        // Format value DateRange start/end dates into the required input format
        // for a daterange type with inclusive bounds.
        // </summary>
        public override void SetValue(IDbDataParameter parameter, IEnumerable<DateRange> value)
        {
            StringBuilder sb = new StringBuilder();
            string formatString = "\"[{0},{1}]\"";
            IEnumerable<string> formattedValues = value
                .Select(range =>
                    string.Format(
                        formatString,
                        range.Start.ToString("yyyy-MM-dd"),
                        range.End.ToString("yyyy-MM-dd")
                    )
                );

            sb.Append("{");
            sb.Append(String.Join(",", formattedValues));
            sb.Append("}");

            parameter.Value = sb.ToString();
        }
    }
}
