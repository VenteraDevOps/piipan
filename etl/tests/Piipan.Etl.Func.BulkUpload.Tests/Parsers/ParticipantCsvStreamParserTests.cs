using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Xunit;
using Moq;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Shared.Utilities;

namespace Piipan.Etl.Func.BulkUpload.Tests.Parsers
{
    public class ParticipantCsvStreamParserTests
    {
        private const string LDS_HASH = "04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef";

        private Stream CsvFixture(string[] records, bool includeHeader = true, bool requiredOnly = false, bool isLegacy = false)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            if (isLegacy)
            {
                writer.WriteLine("lds_hash,case_id,participant_id,benefits_end_month,recent_benefit_issuance_dates,protect_location");
            }
            else
            {
                if (includeHeader)
                {
                    if (requiredOnly)
                    {
                        writer.WriteLine("lds_hash,case_id,participant_id");
                    }
                    else
                    {
                        writer.WriteLine("lds_hash,case_id,participant_id,participant_closing_date,recent_benefit_issuance_dates,protect_location");
                    }
                }
            }
            foreach (var record in records)
            {
                writer.WriteLine(record);
            }
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        [Fact]
        public void ReadAllFields()
        {
            // Arrange
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,2020-10-10,2021-05-01/2021-05-02 2021-06-01/2021-06-02,true"
            });

            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();
            // Assert
            Assert.Single(records);
            Assert.Single(records, r => r.LdsHash == LDS_HASH);
            Assert.Single(records, r => r.CaseId == "CaseId");
            Assert.Single(records, r => r.ParticipantId == "ParticipantId");
            Assert.Single(records, r => r.ParticipantClosingDate == new DateTime(2020, 10, 10));
            DateRange dateRange = records.First().RecentBenefitIssuanceDates.First();
            Assert.Equal(dateRange, new DateRange(new DateTime(2021, 05, 01), new DateTime(2021, 05, 02)));
            Assert.Single(records, r => r.ProtectLocation == true);
        }

        [Fact]
        public void ReadOptionalFields()
        {
            // Arrange
            var stream = CsvFixture(new string[] {
                $"{LDS_HASH},CaseId,ParticipantId,,,,"
            });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
            Assert.Null(records.First().ProtectLocation);
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,,ParticipantId,,,")] // Missing CaseId
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,,,")] // Missing ParticipantId
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,foobar,,")] // Malformed Participant closing date
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,foo2bar,")] // Malformed Recent Benefit Months
        public void ExpectFieldValidationError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.FieldValidationException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,,foobar,")] // Malformed Protect Location
        public void ExpectTypeConverterError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,,")] // Missing last column
        public void ExpectMissingFieldError(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline });
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Throws<CsvHelper.MissingFieldException>(() =>
            {
                foreach (var record in records)
                {
                    ;
                }
            });
        }

        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId")]
        public void OnlyRequiredColumns(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline }, requiredOnly: true);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Equal(LDS_HASH, records.First().LdsHash);
            Assert.Equal("CaseId", records.First().CaseId);
            Assert.Equal("ParticipantId", records.First().ParticipantId);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Null(records.First().ProtectLocation);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
        }

        [Fact]
        public void NullInputStream()
        {
            // Arrange
            Stream stream = null;
            var parser = new ParticipantCsvStreamParser();

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => parser.Parse(stream));
        }

        [Fact]
        public void EmptyInputStream()
        {
            // Arrange
            var stream = CsvFixture(new string[] {}, false, false);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream);

            // Assert
            Assert.Empty(records);
        }
        [Theory]
        [InlineData("04d1117b976e9c894294ab6198bee5fdaac1f657615f6ee01f96bcfc7045872c60ea68aa205c04dd2d6c5c9a350904385c8d6c9adf8f3cf8da8730d767251eef,CaseId,ParticipantId,2020-10,,")]
        public void ExpectLegacyColumns(String inline)
        {
            // Arrange
            var stream = CsvFixture(new string[] { inline }, isLegacy: true);
            var parser = new ParticipantCsvStreamParser();

            // Act
            var records = parser.Parse(stream).ToList();

            // Assert
            Assert.Single(records);
            Assert.Equal(LDS_HASH, records.First().LdsHash);
            Assert.Equal("CaseId", records.First().CaseId);
            Assert.Equal("ParticipantId", records.First().ParticipantId);
            Assert.Null(records.First().ParticipantClosingDate);
            Assert.Null(records.First().ProtectLocation);
            Assert.Empty(records.First().RecentBenefitIssuanceDates);
        }
        

    }
}