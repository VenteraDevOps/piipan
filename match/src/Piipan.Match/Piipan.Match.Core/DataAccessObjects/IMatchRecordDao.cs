using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchRecordDao
    {
        Task<string> AddRecord(MatchRecordDbo record);
        Task<IEnumerable<IMatchRecord>> GetRecords(MatchRecordDbo record);
        Task<IMatchRecord> GetRecordByMatchId(string matchId);
        Task<IEnumerable<IMatchRecord>> GetMatches();
    }
}
