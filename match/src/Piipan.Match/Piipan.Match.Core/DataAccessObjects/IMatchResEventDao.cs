using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;

namespace Piipan.Match.Core.DataAccessObjects
{
    public interface IMatchResEventDao
    {
        Task<int> AddRecord(MatchResEventDbo record);
        Task<IEnumerable<IMatchResEvent>> GetRecords(string matchId, DateTime timestamp);
    }
}
