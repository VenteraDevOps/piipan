using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchResEventDao
    {
        Task<int> AddEvent(MatchResEventDbo record);
        Task<IEnumerable<IMatchResEvent>> GetEvents(
            string matchId,
            bool sortByAsc = true
        );

        Task<IEnumerable<IMatchResEvent>> GetEventsByMatchIDs(
            IEnumerable<string> matchIds,
            bool sortByAsc = true
        );
    }
}
