using Piipan.Dashboard.Pages;
using Xunit;

namespace Piipan.Dashboard.Tests
{
    public class SignedOutPageTests : BasePageTest
    {
        [Fact]
        public void Construct_CallsBasePageConstructor()
        {
            // Arrange
            var serviceProvider = serviceProviderMock();

            // Act
            var page = new SignedOutModel(serviceProvider);

            // Assert
            Assert.Equal("noreply@tts.test", page.Email);
        }
    }
}