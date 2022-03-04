using System.Collections.Generic;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;

namespace Piipan.Match.Core.Builders
{
    public interface IMatchResAggregator
    {
        MatchResRecord Build(
            IMatchRecord match,
            IEnumerable<IMatchResEvent> match_res_events
        );
    }
}
