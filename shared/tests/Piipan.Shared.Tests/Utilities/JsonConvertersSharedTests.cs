using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using Piipan.Shared.Utilities;
using Xunit;

namespace Piipan.Match.Api.Tests.Serializers
{
    public class JsonConvertersSharedTests
    {
        [Fact]
        public void DateTimeConverter_SetsFormat()
        {
            // Arrange
            var converter = new JsonConvertersShared.DateTimeConverter();

            // Assert
            Assert.Equal("yyyy-MM-dd", converter.DateTimeFormat);
        }

    }
}
