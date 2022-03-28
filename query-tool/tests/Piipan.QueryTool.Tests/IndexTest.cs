using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Pages;
using Piipan.QueryTool.Shared;
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class IndexPageTests
    {
        public static IClaimsProvider claimsProviderMock(string email)
        {
            var claimsProviderMock = new Mock<IClaimsProvider>();
            claimsProviderMock
                .Setup(c => c.GetEmail(It.IsAny<ClaimsPrincipal>()))
                .Returns(email);
            return claimsProviderMock.Object;
        }

        public static HttpContext contextMock()
        {
            var request = new Mock<HttpRequest>();

            request
                .Setup(m => m.Scheme)
                .Returns("https");

            request
                .Setup(m => m.Host)
                .Returns(new HostString("tts.test"));

            var context = new Mock<HttpContext>();
            context.Setup(m => m.Request).Returns(request.Object);

            return context.Object;
        }

        [Fact]
        public void TestBeforeOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
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
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
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
                                        RecentBenefitMonths = new List<DateTime>
                                        {
                                            new DateTime(2021, 04, 30),
                                            new DateTime(2021, 03, 31),
                                            new DateTime(2021, 02, 28)
                                        },
                                        ProtectLocation = false
                                    }
                                }
                            }
                        },
                        Errors = new List<OrchMatchError>()
                    }
                });

            var requestPii = new PiiRecord
            {
                //FirstName = "Theodore",
                LastName = "Farrington",
                SocialSecurityNum = "987-65-4320",
                DateOfBirth = new DateTime(1931, 10, 13)
            };

            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<OrchMatchResponse>(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult);
            Assert.NotNull(pageModel.QueryResult.Data.Results[0].Matches);
            Assert.False(pageModel.NoResults);
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
                //FirstName = "Theodore",
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
            };
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.IsType<OrchMatchResponse>(pageModel.QueryResult);
            Assert.Empty(pageModel.QueryResult.Data.Results[0].Matches);
            Assert.True(pageModel.NoResults);
            Assert.Equal("noreply@tts.test", pageModel.Email);
            Assert.Equal("https://tts.test", pageModel.BaseUrl);
        }

        [Fact]
        public async void MatchCapturesApiError()
        {
            // arrange
            var requestPii = new PiiRecord
            {
                //FirstName = "Theodore",
                //MiddleName = "Carri",
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
            };
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = new Mock<IMatchApi>();
            mockMatchApi
                .Setup(m => m.FindMatches(It.IsAny<OrchMatchRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("api broke"));

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi.Object
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // act
            await pageModel.OnPostAsync();

            // assert
            Assert.NotNull(pageModel.RequestError);
            Assert.Equal("There was an error running your search. Please try again.", pageModel.RequestError);
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
                //FirstName = "Theodore",
                //MiddleName = "Carri",
                LastName = "Farrington",
                SocialSecurityNum = "000-00-0000",
                DateOfBirth = new DateTime(2021, 1, 1)
            };
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = new Mock<ILdsDeidentifier>();
            mockLdsDeidentifier
                .Setup(m => m.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(exceptionMessage));

            var mockMatchApi = Mock.Of<IMatchApi>();

            var pageModel = new IndexModel(
                new NullLogger<IndexModel>(),
                mockClaimsProvider,     
                mockLdsDeidentifier.Object,
                mockMatchApi
            );
            pageModel.Query = requestPii;
            pageModel.PageContext.HttpContext = contextMock();

            // Act
            await pageModel.OnPostAsync();

            // Assert
            Assert.NotNull(pageModel.RequestError);
            Assert.Equal(expectedErrorMessage, pageModel.RequestError);
        }
    }
}
