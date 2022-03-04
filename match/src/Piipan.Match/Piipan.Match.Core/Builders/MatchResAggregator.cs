using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Piipan.Match.Core.Builders
{
    /// <summary>
    /// Builder for MatchResRecord objects
    /// </summary>
    public class MatchResAggregator : IMatchResAggregator
    {
        /// <summary>
        /// Builds a MatchResRecord object from match and match_res_events data
        /// </summary>
        /// <param name="matchId">The match ID for match and related match res events</param>
        public MatchResRecord Build(
            IMatchRecord match,
            IEnumerable<IMatchResEvent> match_res_events
        )
        {
            match_res_events = match_res_events.OrderBy(e => e.InsertedAt);
            string jsonString = MergeEvents(match_res_events);
            var aggregate = JsonConvert.DeserializeObject<MatchResRecord>(jsonString);
            aggregate.Dispositions = AggregateDispositions(match, match_res_events);
            // TODO: aggregate state participant data. This will need to come from whatever json schema we have on match query and data fields
            return aggregate;
        }

        private Disposition[] AggregateDispositions(
            IMatchRecord match,
            IEnumerable<IMatchResEvent> match_res_events
        )
        {
            var collect = new List<Disposition>();

            foreach (var stateAbbr in match.States)
            {
                var mreFiltered = match_res_events.Where(mre => mre.ActorState == stateAbbr);
                string jsonString = MergeEvents(mreFiltered);
                var stateObj = JsonConvert.DeserializeObject<Disposition>(jsonString);
                stateObj.State = stateAbbr;
                collect.Add(stateObj);
            }
            return collect.ToArray();
        }

        private string MergeEvents(IEnumerable<IMatchResEvent> match_res_events)
        {
            return match_res_events
                .Select(mre => JObject.Parse(mre.Delta))
                .Aggregate(JObject.Parse(@"{}"), (acc, x) => {
                    acc.Merge(x, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                    return acc;
                })
                .ToString();
        }
    }
}
