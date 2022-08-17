using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Pages;
using Piipan.QueryTool.Tests.Extensions;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class IndexPageTests : BasePageTest
    {


        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi,
                mockServiceProvider
            );

            // act

            // assert
            Assert.Equal("", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public async Task TestAfterOnGet()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi,
                mockServiceProvider
            );
            pageModel.PageContext.HttpContext = contextMock();
            var renderer = SetupRenderingApi();

            // act
            await OnPageHandlerExecutionAsync(pageModel, "OnGet");
            pageModel.OnGet();
            var (page, output) = await renderer.RenderPage("/Pages/Index.cshtml", pageModel);

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
            Assert.NotEmpty(pageModel.StateInfo.Results);
            Assert.Equal("NAC Participant Search", page.ViewContext.ViewData["Title"]);
            Assert.Contains("Piipan.QueryTool.Client.Components.QueryForm", output);
        }

        [Fact]
        public async Task MatchSetsResults()
        {
            // arrange
            var mockMatchApi = new Mock<IMatchApi>();
            mockMatchApi
                .Setup(m => m.FindMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new OrchMatchResponse
                {
                    Data = new OrchMatchResponseData
                    {
                        Results = new List<OrchMatchResult>
                        {
                            new OrchMatchResult
                            {
                                Index = 0,
                                Matches = new List<ParticipantMatch>
                                {
                                    new ParticipantMatch
                                    {
                                        LdsHash = "foobar",
                                        State = "ea",
                                        CaseId = "caseId",
                                        ParticipantId = "pId",
                                        ParticipantClosingDate = new DateTime(2021, 05, 31),
                                        RecentBenefitIssuanceDates = new List<DateRange>
                                        {
                                            new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                                            new DateRange(new DateTime(2021, 6, 1),new DateTime(2021, 7, 1)),
                                            new DateRange(new DateTime(2021, 02, 28),new DateTime(2021, 3, 15))
                                        },
                                        VulnerableIndividual = false
                                    }
                                }
                            }
                        },
                        Errors = new List<OrchMatchError>()
                    }
                });

            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "887-65-4320",
                DateOfBirth = new DateTime(1931, 10, 13),
                ParticipantId = "participantid1",
                SearchReason = "application"
            };

            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi.Object,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<OrchMatchResponseData>(pageModel.QueryFormData.QueryResult);
            Assert.NotNull(pageModel.QueryFormData.QueryResult);
            Assert.NotNull(pageModel.QueryFormData.QueryResult.Results[0].Matches);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task MatchNoResults()
        {
            // arrange
            var mockMatchApi = new Mock<IMatchApi>();
            mockMatchApi
                .Setup(m => m.FindMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ReturnsAsync(new OrchMatchResponse
                {
                    Data = new OrchMatchResponseData
                    {
                        Results = new List<OrchMatchResult>
                        {
                            new OrchMatchResult
                            {
                                Index = 0,
                                Matches = new List<ParticipantMatch>()
                            }
                        },
                        Errors = new List<OrchMatchError>()
                    }
                });

            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "111-11-1111",
                DateOfBirth = new DateTime(2021, 1, 1),
                ParticipantId = "participantid1",
                SearchReason = "new_household_member"
            };
            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi.Object,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<OrchMatchResponseData>(pageModel.QueryFormData.QueryResult);
            Assert.Empty(pageModel.QueryFormData.QueryResult.Results[0].Matches);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task MatchCapturesInvalidStateErrors()
        {
            // arrange
            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000", // social security number is invalid 3 times
                DateOfBirth = new DateTime(2021, 1, 1),
                ParticipantId = "participantid1",
                SearchReason = "other"
            };
            var mockServiceProvider = serviceProviderMock(location: "EA");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = new Mock<IMatchApi>();

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi.Object,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.QueryFormData.ServerErrors);
            Assert.Equal(new List<ServerError> {
                new ServerError("QueryFormData.Query.SocialSecurityNum",
                "The first three numbers of @@@ cannot be 000\nThe middle two numbers of @@@ cannot be 00\nThe last four numbers of @@@ cannot be 0000")
            }, pageModel.QueryFormData.ServerErrors);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task MatchCapturesInvalidLocationError()
        {
            // arrange
            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "111-11-1111",
                DateOfBirth = new DateTime(2021, 1, 1),
                ParticipantId = "participantid1",
                SearchReason = "recertification"
            };
            var mockServiceProvider = serviceProviderMock(location: "National");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = new Mock<IMatchApi>();

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi.Object,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.QueryFormData.ServerErrors);
            Assert.Equal(new List<ServerError> {
                new ServerError("", "Search performed with an invalid location") }, pageModel.QueryFormData.ServerErrors);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async Task MatchCapturesApiError()
        {
            // arrange
            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "111-11-1111",
                DateOfBirth = new DateTime(2021, 1, 1),
                ParticipantId = "participantid1",
                SearchReason = "other"
            };
            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = new Mock<IMatchApi>();
            mockMatchApi
                .Setup(m => m.FindMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("api broke"));

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi.Object,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.QueryFormData.ServerErrors);
            Assert.Equal(new List<ServerError> {
                new ServerError("", "There was an error running your search. Please try again.") }, pageModel.QueryFormData.ServerErrors);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Theory]
        [InlineData("something gregorian something", "Date of birth must be a real date.")]
        [InlineData("something something something", "something something something")]
        public async Task InvalidDateFormat(string exceptionMessage, string expectedErrorMessage)
        {
            // Arrange
            var requestPii = new PiiQuery
            {
                LastName = "Farrington",
                SocialSecurityNum = "111-11-1111",
                DateOfBirth = new DateTime(2021, 1, 1),
                ParticipantId = "participantid1",
                SearchReason = "application"
            };
            var mockServiceProvider = serviceProviderMock();
            var mockLdsDeidentifier = new Mock<ILdsDeidentifier>();
            mockLdsDeidentifier
                .Setup(m => m.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(exceptionMessage));

            var mockMatchApi = Mock.Of<IMatchApi>();

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier.Object,
                mockMatchApi,
                mockServiceProvider
            );
            pageModel.QueryFormData.Query = requestPii;
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // Act
            await pageModel.OnPostAsync();

            // Assert
            Assert.NotNull(pageModel.QueryFormData.ServerErrors);
            Assert.Equal(new List<ServerError> {
                new ServerError("", expectedErrorMessage) }, pageModel.QueryFormData.ServerErrors);
        }

        [Fact]
        public async Task PageStillLoadsIfStatesApiErrors()
        {
            // arrange
            var mockServiceProvider = serviceProviderMock(statesInfoResponseOverride: (i) => i.ThrowsAsync(new Exception("Test exception")));
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi,
                mockServiceProvider
            );
            pageModel.BindModel(pageModel.QueryFormData.Query, $"{nameof(IndexModel.QueryFormData)}.{nameof(IndexModel.QueryFormData.Query)}",
                validationContext: new ValidationContext(pageModel.QueryFormData.Query));
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await OnPageHandlerExecutionAsync(pageModel, "OnGet");
            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
            Assert.Empty(pageModel.StateInfo.Results);
        }
    }
}
