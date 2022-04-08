using Dapper;
using NpgsqlTypes;
using Piipan.Shared.API.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
        public override IEnumerable<DateRange> Parse(object value)
        {
            IEnumerable<DateRange> typedValue = value is DBNull ? new List<DateRange>() : ((NpgsqlRange<DateTime>[])value).Select(user => new DateRange() { Start = user.LowerBound, End = user.UpperBound }).ToList();
            return typedValue;
        }

        public override void SetValue(IDbDataParameter parameter, IEnumerable<DateRange> value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            string formatString = "\"[{0},{1})\",";
            foreach (DateRange Dat in value)
            {
                sb.Append(string.Format(formatString, Dat.Start.ToString("yyyy-MM-dd"), Dat.End.ToString("yyyy-MM-dd")));
            }
            sb.Remove(sb.Length - 1, 1); 
            sb.Append("}");

            parameter.Value = sb.ToString();
          }
    }
}
