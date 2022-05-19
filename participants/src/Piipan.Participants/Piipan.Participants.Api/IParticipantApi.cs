using Piipan.Participants.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash);
        Task AddParticipants(IEnumerable<IParticipant> participants, string uploadIdentifier, string fileName);
        Task<IEnumerable<string>> GetStates();
        Task DeleteOldParticpants(string state = null);
    }
}