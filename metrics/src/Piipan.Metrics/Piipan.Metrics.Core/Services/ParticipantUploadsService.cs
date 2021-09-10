using System;
using System.Collections.Generic;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Api;

namespace Piipan.Metrics.Core.Services
{
    public class ParticipantUploadService : IParticipantUploadApi
    {
        private readonly IParticipantUploadDao _participantUploadDao;

        public ParticipantUploadService(IParticipantUploadDao participantUploadDao)
        {
            _participantUploadDao = participantUploadDao;
        }

        public Int64 GetUploadCount(string? state)
        {
            return _participantUploadDao.GetUploadCount(state);
        }

        public IEnumerable<ParticipantUpload> GetUploads(string? state, int limit, int offset = 0)
        {
            return _participantUploadDao.GetUploads(state, limit, offset);
        }

        public IEnumerable<ParticipantUpload> GetLatestUploadsByState()
        {
            throw new NotImplementedException();
        }
    }
}