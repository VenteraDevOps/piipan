using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Extensions;
using Piipan.Match.Core.Parsers;
using Piipan.Match.Core.Services;
using Piipan.Match.Core.Validators;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Extensions;
using Piipan.Participants.Core.Models;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Xunit;


namespace Piipan.Match.Func.Api.IntegrationTests
{
    public class ApiIntegrationTests : DbFixture
    {
        private string InitiatingState = "eb";
        private const string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";

        static ParticipantDbo FullRecord()
        {
            return new ParticipantDbo
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec",
                CaseId = "CaseIdExample",
                ParticipantId = "participantid1",
                ParticipantClosingDate = new DateTime(1970, 1, 15),
                RecentBenefitIssuanceDates = new List<DateRange>
                {
                    new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                    new DateRange(new DateTime(2021, 6, 1),new DateTime(2021, 7, 1)),
                    new DateRange(new DateTime(2021, 02, 28),new DateTime(2021, 3, 15))
                },
                VulnerableIndividual = true
            };
        }
        static ParticipantDbo EncryptedFullRecord(ParticipantDbo unencryptedRecord)
        {
            AzureAesCryptographyClient client = new AzureAesCryptographyClient(base64EncodedKey);

            return new ParticipantDbo
            {
                // farrington,1931-10-13,000-12-3456
                LdsHash = client.EncryptToBase64String(unencryptedRecord.LdsHash),
                CaseId = client.EncryptToBase64String(unencryptedRecord.CaseId),
                ParticipantId = client.EncryptToBase64String(unencryptedRecord.ParticipantId),
                ParticipantClosingDate = unencryptedRecord.ParticipantClosingDate,
                RecentBenefitIssuanceDates = unencryptedRecord.RecentBenefitIssuanceDates,
                VulnerableIndividual = unencryptedRecord.VulnerableIndividual
            };
        }

        static String JsonBody(object[] records)
        {
            var data = new
            {
                data = records
            };

            return JsonConvert.SerializeObject(data);
        }

        Mock<HttpRequest> MockRequest(string jsonBody)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(jsonBody);
            sw.Flush();

            ms.Position = 0;

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(x => x.Body).Returns(ms);

            mockRequest
                .Setup(x => x.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "From", "a user" },
                    { "Ocp-Apim-Subscription-Name", "sub-name" },
                    { "X-Initiating-State", InitiatingState }
                }));

            return mockRequest;
        }

        static MatchApi Construct()
        {
            Environment.SetEnvironmentVariable("States", "ea");
            Environment.SetEnvironmentVariable("EventGridEndPoint","http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString","example");
            

            // Mixing cases to verify the enabled states can be used no matter their casing.
            Environment.SetEnvironmentVariable("EnabledStates", "ea,EB");

            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddTransient<IValidator<OrchMatchRequest>, OrchMatchRequestValidator>();
            services.AddTransient<IValidator<RequestPerson>, RequestPersonValidator>();

            services.AddTransient<IStreamParser<OrchMatchRequest>, OrchMatchRequestParser>();

            services.AddTransient<IMatchResEventDao, MatchResEventDao>();
            services.AddTransient<IMatchResAggregator, MatchResAggregator>();
            services.AddSingleton<IMemoryCache, MemoryCache>();

            services.AddTransient<IDbConnectionFactory<ParticipantsDb>>(s =>
            {
                return new BasicPgConnectionFactory<ParticipantsDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.DatabaseConnectionString));
            });
            services.RegisterParticipantsServices();
            services.RegisterMatchServices();
            services.RegisterKeyVaultClientServices();

            services.AddTransient<IDbConnectionFactory<CollaborationDb>>(s =>
            {
                return new BasicPgConnectionFactory<CollaborationDb>(
                    NpgsqlFactory.Instance,
                    Environment.GetEnvironmentVariable(Startup.CollaborationDatabaseConnectionString));
            });

            var provider = services.BuildServiceProvider();

            var api = new MatchApi(
                provider.GetService<IMatchApi>(),
                provider.GetService<IStreamParser<OrchMatchRequest>>(),
                provider.GetService<IMatchEventService>(),
                provider.GetService<IMemoryCache>()
            );

            return api;
        }

        [Fact]
        public async void ApiReturnsMatches()
        {
            // Arrange
            var record = FullRecord();
            var recordEncrypted = EncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            Insert(recordEncrypted);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);

            var person = resultObject.Data.Results.First();
            Assert.Equal(record.CaseId, person.Matches.First().CaseId);
            Assert.Equal(record.ParticipantId, person.Matches.First().ParticipantId);
            Assert.Equal(state[0], person.Matches.First().State);
            Assert.Equal(recordEncrypted.ParticipantClosingDate, person.Matches.First().ParticipantClosingDate);
            Assert.Equal(recordEncrypted.VulnerableIndividual, person.Matches.First().VulnerableIndividual);
            // serialization
            var match = person.Matches.First();
            var json = JsonConvert.SerializeObject(match);
            Assert.Contains("participant_id", json);
            Assert.Contains("recent_benefit_issuance_dates", json);
        }

        [Fact]
        public async void ApiReturnsEmptyMatchesArray()
        {
            // Arrange
            var record = FullRecord();
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);
        }

        [Fact]
        public async void ApiReturnsEmptyMatchesArrayWhenStateDisabled()
        {
            // Arrange
            var record = FullRecord();
            var recordEncrypted = EncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, SearchReason = "other" } };
            InitiatingState = "ec"; // set to a state that is disabled
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordEncrypted);

            // Assert database is empty prior to the call
            ClearMatchRecords();
            var matchesBefore = GetMatches();
            Assert.Empty(matchesBefore);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Empty(resultObject.Data.Results[0].Matches);

            // Assert a match was actually created even though we got an empty array back
            var matchesAfter = GetMatches();
            Assert.NotEmpty(matchesAfter);
        }

        [Fact]
        public async void ApiReturnsInlineErrors()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            recordB.LdsHash = "foo";
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, SearchReason = "other" },
                new RequestPerson {LdsHash = recordB.LdsHash, SearchReason = "other" }
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();

            ClearParticipants();
            Insert(recordA);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            Assert.Single(resultObject.Data.Results);
            Assert.Single(resultObject.Data.Errors);
        }

        [Fact]
        public async void ApiReturnsExpectedIndices()
        {
            // Arrange
            var recordA = FullRecord();
            var recordB = FullRecord();
            var recordEncryptedA = EncryptedFullRecord(recordA);

            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var recordEncryptedB = EncryptedFullRecord(recordB);
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, SearchReason = "other" },
                new RequestPerson {LdsHash = recordB.LdsHash, SearchReason = "other" }
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            InsertUpload();
            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncryptedA);
            Insert(recordEncryptedB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            // Assert
            var resultA = resultObject.Data.Results.Find(p => p.Index == 0);
            var resultB = resultObject.Data.Results.Find(p => p.Index == 1);

            AzureAesCryptographyClient cryptographyClient = new AzureAesCryptographyClient(base64EncodedKey);

            Assert.Equal(resultA.Matches.First().ParticipantId, recordA.ParticipantId);
            Assert.Equal(resultB.Matches.First().ParticipantId, recordB.ParticipantId);
        }
        
        [Fact]
        public async void ApiCreatesMatchRecords()
        {
            // Arrange
            var record = FullRecord();
            var recordEncrypted = EncryptedFullRecord(record);
            var logger = Mock.Of<ILogger>();
            var body = new object[] { new RequestPerson { LdsHash = record.LdsHash, SearchReason = "other" } };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncrypted);

            // Act
            var response = await api.Find(mockRequest.Object, logger);

            // Assert
            Assert.Equal(1, CountMatchRecords());
        }

        [Fact]

        public async void ApiCreatesMatchRecordsWithCorrectValues()
        {
            // Arrange
            var recordA = FullRecord();
            var recordEncryptedA = EncryptedFullRecord(recordA);
            
            var recordB = FullRecord();
            // lynn,1940-08-01,000-12-3457
            recordB.LdsHash = "97719c32bb3c6a5e08c1241a7435d6d7047e75f40d8b3880744c07fef9d586954f77dc93279044c662d5d379e9c8a447ce03d9619ce384a7467d322e647e5d95";
            recordB.ParticipantId = "ParticipantB";
            var recordEncryptedB = EncryptedFullRecord(recordB);
            var logger = Mock.Of<ILogger>();
            var body = new object[] {
                new RequestPerson { LdsHash = recordA.LdsHash, SearchReason = "other" },
                new RequestPerson { LdsHash = recordB.LdsHash, SearchReason = "other" },
            };
            var mockRequest = MockRequest(JsonBody(body));
            var api = Construct();
            var state = Environment.GetEnvironmentVariable("States").Split(",");

            ClearParticipants();
            ClearMatchRecords();
            Insert(recordEncryptedA);
            Insert(recordEncryptedB);

            // Act
            var response = await api.Find(mockRequest.Object, logger);
            var result = response as JsonResult;
            var resultObject = result.Value as OrchMatchResponse;

            AzureAesCryptographyClient cryptoClient = new AzureAesCryptographyClient(base64EncodedKey);

            // Assert
            Assert.All(resultObject.Data.Results, result =>
            {
                var match = result.Matches.First();
                var record = GetMatchRecord(match.MatchId);

                string encryptedLdsHashOfMatch = cryptoClient.EncryptToBase64String(match.LdsHash);
                Assert.Equal(InitiatingState, record.Initiator);
                Assert.True(record.States.SequenceEqual(new string[] { InitiatingState, state[0] }));
                Assert.Equal(encryptedLdsHashOfMatch, record.Hash);
                Assert.Equal("ldshash", record.HashType);
            });
        }
    }
}
