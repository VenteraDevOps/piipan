using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.QueryTool.Pages;
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
        public void TestAfterOnGet()
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

            // act

            pageModel.OnGet();

            // assert
            Assert.Equal("NAC Query Tool", pageModel.Title);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async void MatchSetsResults()
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

            var requestPii = new PiiRecord
            {
                LastName = "Farrington",
                SocialSecurityNum = "987-65-4320",
                DateOfBirth = new DateTime(1931, 10, 13)
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
        public async void MatchNoResults()
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

            var requestPii = new PiiRecord
            {
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
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
        public async void MatchCapturesInvalidLocationError()
        {
            // arrange
            var requestPii = new PiiRecord
            {
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
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
        public async void MatchCapturesApiError()
        {
            // arrange
            var requestPii = new PiiRecord
            {
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
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
            var requestPii = new PiiRecord
            {
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
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
            pageModel.PageContext.HttpContext = contextMock();

            // Act
            await pageModel.OnPostAsync();

            // Assert
            Assert.NotNull(pageModel.QueryFormData.ServerErrors);
            Assert.Equal(new List<ServerError> {
                new ServerError("", expectedErrorMessage) }, pageModel.QueryFormData.ServerErrors);
        }

        [Theory]
        [InlineData(nameof(IndexModel.OnGet), null, null, false)]
        [InlineData(nameof(IndexModel.OnGet), "IA", null, false)]
        [InlineData(nameof(IndexModel.OnGet), null, "Worker", false)]
        [InlineData(nameof(IndexModel.OnGet), "IA", "Worker", true)]
        [InlineData(nameof(IndexModel.OnPostAsync), null, null, false)]
        [InlineData(nameof(IndexModel.OnPostAsync), "IA", null, false)]
        [InlineData(nameof(IndexModel.OnPostAsync), null, "Worker", false)]
        [InlineData(nameof(IndexModel.OnPostAsync), "IA", "Worker", true)]
        public void IsAccessibleWhenRolesExist(string method, string role, string location, bool isAuthorized)
        {
            var mockServiceProvider = serviceProviderMock(location: location, role: role);

            var pageHandlerExecutingContext = GetPageHandlerExecutingContext(mockServiceProvider, method);

            if (!isAuthorized)
            {
                Assert.Equal(403, pageHandlerExecutingContext.HttpContext.Response.StatusCode);
                Assert.IsType<RedirectToPageResult>(pageHandlerExecutingContext.Result);
            }
            else
            {
                Assert.Equal(200, pageHandlerExecutingContext.HttpContext.Response.StatusCode);
            }
        }

        private PageHandlerExecutingContext GetPageHandlerExecutingContext(IServiceProvider serviceProvider, string methodName)
        {
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockLdsDeidentifier,
                mockMatchApi,
                serviceProvider
            );
            return base.GetPageHandlerExecutingContext(pageModel, methodName);
        }
    }
}
