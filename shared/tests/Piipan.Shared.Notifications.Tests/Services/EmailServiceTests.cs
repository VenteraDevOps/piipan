using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Notifications.Models;
using Piipan.Shared.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.Shared.Notifications.Tests.Services
{
    public class EmailServiceTests
    {
        ///// <summary>
        ///// This code will try to send and email provided you replace the from with a valid google workspace email that also has "Less secure app access" set to ON
        ///// </summary>
        //[Fact]
        //public async void SuccessfulSendEmailSingleRecipient()
        //{
        //    // Arrange
        //    var logger = Mock.Of<ILogger<EmailService>>();
        //    var randomLdsHash = Guid.NewGuid().ToString();
        //    var emailModel = new EmailModel
        //    {
        //        Body = "Test email body.",
        //        ToList = new List<string>
        //        {
        //             "ccaprio@ventera.com"
        //        },
        //        From = "ccaprio.adm1n@ventera-nac-test.com",
        //        Subject = "Test Email From Email Service Tests"

        //    };

        //    var emailService = new EmailService("smtp.gmail.com", "ccaprio.adm1n@ventera-nac-test.com", "Deadbeef123!", logger);

        //    // Act
        //    var result = await emailService.SendEmail(emailModel);

        //    // Assert
        //    Assert.True(result);
        //}

        ///// <summary>
        ///// This code will try to send and email provided you replace the from with a valid google workspace email that also has "Less secure app access" set to ON
        ///// </summary>
        //[Fact]
        //public async void SuccessfulSendEmailMultipleRecipienst()
        //{
        //    // Arrange
        //    var logger = Mock.Of<ILogger<EmailService>>();
        //    var randomLdsHash = Guid.NewGuid().ToString();
        //    var emailModel = new EmailModel
        //    {
        //        Body = "Test email body.",
        //        ToList = new List<string>
        //        {
        //             "ccaprio@ventera.com",
        //             "bfoshay@ventera.com"
        //        },
        //        From = "ccaprio.adm1n@ventera-nac-test.com",
        //        Subject = "Test Email From Email Service Tests"

        //    };

        //    var emailService = new EmailService("smtp.gmail.com", "ccaprio.adm1n@ventera-nac-test.com", "Deadbeef123!", logger);

        //    // Act
        //    var result = await emailService.SendEmail(emailModel);

        //    // Assert
        //    Assert.True(result);
        //}

        [Fact]
        public async void FailedSendEmail()
        {
            // Arrange
            var logger = Mock.Of<ILogger<EmailService>>();
            var randomLdsHash = Guid.NewGuid().ToString();
            var emailModel = new EmailModel
            {
                Body = "Test email body.",
                ToList = new List<string>
                {
                     "bfoshay@ventera.com"
                },
                From = "ccaprio.adm1n@ventera-nac-test.com",
                Subject = "Test Email From Email Service Tests"

            };

            var emailService = new EmailService("smtp.gmail.com", "bad-account@gmail.com", "FalsePassword", logger);

            // Act
            var result = await emailService.SendEmail(emailModel);

            // Assert
            Assert.False(result);
        }
    }
}
