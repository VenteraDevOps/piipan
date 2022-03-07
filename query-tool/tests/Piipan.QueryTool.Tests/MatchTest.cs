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
using Piipan.Shared.Claims;
using Piipan.Shared.Deidentification;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace Piipan.QueryTool.Tests
{
    public class MatchPageTests
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
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void TestShortCaseId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(pageModel.OnGet("m12345")).PageName;

            // assert
            Assert.Equal("/NotFound", result);
        }

        [Fact]
        public void TestInvalidCharactersOnCaseId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var result = Assert.IsType<RedirectToPageResult>(pageModel.OnGet("m1$23^45")).PageName;

            // assert
            Assert.Equal("/NotFound", result);
        }

        [Fact]
        public void TestValidCaseId()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new MatchModel(
                new NullLogger<MatchModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );
            pageModel.PageContext.HttpContext = contextMock();

            // act
            var caseid = "m123456";
            // var result = Assert.IsType<RedirectToPageResult>(pageModel.OnGet("m123456")).PageName;
            pageModel.OnGet(caseid);
            var result = pageModel.Match.MatchId;

            // assert
            Assert.Equal(caseid, result);
        }
    }
}
