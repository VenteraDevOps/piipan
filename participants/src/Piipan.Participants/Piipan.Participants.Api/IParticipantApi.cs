using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash);
        Task AddParticipants(IEnumerable<IParticipant> participants, string uploadIdentifier, Action<Exception> errorCallback);
        Task<IEnumerable<string>> GetStates();
        Task DeleteOldParticpants(string state = null);

        void LogParticipantsUploadError(ParticipantUploadErrorDetails errorDetails, IEnumerable<IParticipant> participants);
    }
}