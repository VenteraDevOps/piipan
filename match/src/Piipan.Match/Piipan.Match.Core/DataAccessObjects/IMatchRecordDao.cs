using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchRecordDao
    {
        Task<string> AddRecord(MatchRecordDbo record);
        Task<IEnumerable<IMatchRecord>> GetRecords(MatchRecordDbo record);
        Task<IMatchRecord> GetRecordByMatchId(string matchId);
    }
}
