using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public interface IParticipantBulkInsertHandler
    {
        Task LoadParticipants(IEnumerable<ParticipantDbo> records, IDbConnection connection, string tableName);
    }
}
