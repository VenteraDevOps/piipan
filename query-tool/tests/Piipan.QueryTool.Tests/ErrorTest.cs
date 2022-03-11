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
    public class ErrorPageTests
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
            var pageModel = new ErrorModel(
                new NullLogger<ErrorModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );

            // act

            // assert
            Assert.Equal("noreply@tts.test", pageModel.Email);
        }

        [Fact]
        public void TestMessageOnGet()
        {
            // arrange
            var mockClaimsProvider = claimsProviderMock("noreply@tts.test");
            var mockLdsDeidentifier = Mock.Of<ILdsDeidentifier>();
            var mockMatchApi = Mock.Of<IMatchApi>();
            var pageModel = new ErrorModel(
                new NullLogger<ErrorModel>(),
                mockClaimsProvider,
                mockLdsDeidentifier,
                mockMatchApi
            );

            // act
            string message = "test message";
            pageModel.OnGet(message);
            // assert
            Assert.Equal(message, pageModel.Message);
        }

        
    }
}