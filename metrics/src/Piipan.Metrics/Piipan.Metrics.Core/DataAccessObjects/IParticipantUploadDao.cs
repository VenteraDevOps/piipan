using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantUploadDao
    {
        Task<Int64> GetUploadCount(string? state);
        Task<IEnumerable<ParticipantUpload>> GetUploads(string? state, int limit, int offset = 0);
        Task<IEnumerable<ParticipantUpload>> GetLatestSuccessfulUploadsByState();

        Task<int> AddUpload(ParticipantUploadDbo newUploadDbo);
        Task<int> UpdateUpload(ParticipantUploadDbo uploadDbo);
    }
}