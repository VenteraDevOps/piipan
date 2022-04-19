using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash);
        Task<BulkUploadResponse> AddParticipants(IEnumerable<IParticipant> participants, string uploadIdentifier);
        Task<IEnumerable<string>> GetStates();
    }
}