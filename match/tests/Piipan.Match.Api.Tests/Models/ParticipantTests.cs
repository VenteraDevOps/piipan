using Newtonsoft.Json;
using Piipan.Match.Api.Models;
using Xunit;

namespace Piipan.Match.Api.Tests.Models
{
    public class ParticipantTests
    {
        [Fact]
        public void ParticipantRecordJson()
        {
            // Arrange
            var json = @"{participant_id: 'baz', case_id: 'foo', participant_closing_date: '2020-01-12', recent_benefit_months: ['2019-12', '2019-11', '2019-10'], protect_location: true}";
            var record = JsonConvert.DeserializeObject<ParticipantMatch>(json);

            string jsonRecord = record.ToJson();

            Assert.Contains("\"state\": null", jsonRecord);
            Assert.Contains("\"participant_id\": \"baz\"", jsonRecord);
            Assert.Contains("\"case_id\": \"foo\"", jsonRecord);
            Assert.Contains("\"participant_closing_date\": \"2020-01-12\"", jsonRecord);
            Assert.Contains("\"recent_benefit_months\": [", jsonRecord);
            Assert.Contains("\"2019-12\",", jsonRecord);
            Assert.Contains("\"2019-11\",", jsonRecord);
            Assert.Contains("\"2019-10\"", jsonRecord);
            Assert.Contains("\"protect_location\": true", jsonRecord);
        }
    }
}
