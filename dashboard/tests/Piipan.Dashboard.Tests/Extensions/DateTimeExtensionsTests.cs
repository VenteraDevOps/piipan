using System;
using System.Globalization;
using Piipan.Dashboard.Client.Helpers;
using ReflectionMagic;
using Xunit;

namespace Piipan.Dashboard.Tests.Extensions
{
    public class FakeLocalTimeZone : IDisposable
    {
        private readonly TimeZoneInfo _actualLocalTimeZoneInfo;

        private static void SetLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            typeof(TimeZoneInfo).AsDynamicType().s_cachedData._localTimeZone = timeZoneInfo;
        }

        public FakeLocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            _actualLocalTimeZoneInfo = TimeZoneInfo.Local;
            SetLocalTimeZone(timeZoneInfo);
        }

        public void Dispose()
        {
            SetLocalTimeZone(_actualLocalTimeZoneInfo);
        }
    }
    public class DateTimeExtensionsTests
    {
        [Fact]
        public void TestFakeLocalTimeZone()
        {
            var testTimeZone = TimeZoneInfo.CreateCustomTimeZone("t1", TimeSpan.FromHours(-5), "Testing Time", "Testing Time");
            using (new FakeLocalTimeZone(testTimeZone))
            {
                var dateTime = DateTimeOffset.ParseExact("20220101120000+00:00", "yyyyMMddHHmmsszzz", CultureInfo.InvariantCulture);
                // Daylight savings time EST is -5
                Assert.Equal($"1/1/2022 7:00:00 AM TT", dateTime.DateTime.ToFullTimeWithTimezone());
            }
        }
    }
}
