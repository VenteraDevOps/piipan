using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Helpers;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    /// <summary>
    /// Maps and validates a record from a CSV file formatted in accordance with
    /// <c>/etl/docs/csv/import-schema.json</c> to a <c>IParticipant</c>.
    /// </summary>
    public class ParticipantMap : ClassMap<Participant>
    {
        public ParticipantMap()
        {
            Map(m => m.LdsHash).Name("lds_hash").Validate(field =>
            {
                Match match = Regex.Match(field.Field, "^[0-9a-f]{128}$");
                return match.Success;
            });

            Map(m => m.CaseId).Name("case_id").Validate(field =>
            {
                return !string.IsNullOrEmpty(field.Field);
            });

            Map(m => m.ParticipantId).Name("participant_id").Validate(field =>
            {
                return !string.IsNullOrEmpty(field.Field);
            });

            Map(m => m.ParticipantClosingDate)
                .Name("participant_closing_date")
                .Validate(field =>
                {
                    if (String.IsNullOrEmpty(field.Field)) return true;

                    string[] formats = { "yyyy-MM-dd"};
                    DateTime dateValue;
                    var result = DateTime.TryParseExact(
                        field.Field,
                        formats,
                        new CultureInfo("en-US"),
                        DateTimeStyles.None,
                        out dateValue);
                    if (!result) return false;
                    return true;
                })
                .TypeConverterOption.NullValues(string.Empty).Optional();

            Map(m => m.RecentBenefitMonths)
                .Name("recent_benefit_months")
                .Validate(field => {
                    if (String.IsNullOrEmpty(field.Field)) return true;

                    string[] formats={"yyyy-MM", "yyyy-M"};
                    string[] dates = field.Field.Split(' ');
                    foreach (string date in dates)
                    {
                        DateTime dateValue;
                        var result = DateTime.TryParseExact(
                            date,
                            formats,
                            new CultureInfo("en-US"),
                            DateTimeStyles.None,
                            out dateValue);
                        if (!result) return false;
                    }
                    return true;
                })
                .TypeConverter<ToMonthEndArrayConverter>().Optional();

            Map(m => m.ProtectLocation).Name("protect_location")
                .TypeConverterOption.NullValues(string.Empty).Optional();

        }
    }
   
    /// <summary>
    /// Converts list of month-only dates to last day of month when as DateTimes
    /// and to ISO 8601 year-months when as a string
    /// </summary>
  	public class ToMonthEndArrayConverter : DefaultTypeConverter
  	{
      	public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
      	{
			if (text == "") return new List<DateTime>();
			string[] allElements = text.Split(' ');
			DateTime[] elementsAsDateTimes = allElements.Select(s => MonthEndDateTime.Parse(s)).ToArray();
			return new List<DateTime>(elementsAsDateTimes);
      	}
  	}

    public class ParticipantCsvStreamParser : IParticipantStreamParser
    {
        public IEnumerable<IParticipant> Parse(Stream input)
        {
            var reader = new StreamReader(input);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ParticipantMap>();

            // Yields records as it is iterated over
            return csv.GetRecords<Participant>();
        }
    }
}
