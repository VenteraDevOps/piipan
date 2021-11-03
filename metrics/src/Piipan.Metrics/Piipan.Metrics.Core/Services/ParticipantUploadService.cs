using System;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Api;
using System.Threading.Tasks;
using Piipan.Metrics.Core.Builders;

#nullable enable

namespace Piipan.Metrics.Core.Services
{
    public class ParticipantUploadService : IParticipantUploadReaderApi, IParticipantUploadWriterApi
    {
        private readonly IParticipantUploadDao _participantUploadDao;
        private readonly IMetaBuilder _metaBuilder;

        public ParticipantUploadService(IParticipantUploadDao participantUploadDao, IMetaBuilder metaBuilder)
        {
            _participantUploadDao = participantUploadDao;
            _metaBuilder = metaBuilder;
        }

        public async Task<GetParticipantUploadsResponse> GetLatestUploadsByState()
        {
            var uploads = await _participantUploadDao.GetLatestUploadsByState();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = _metaBuilder.Build()
            };
        }

        public async Task<int> AddUpload(string state, DateTime uploadedAt)
        {
            return await _participantUploadDao.AddUpload(state, uploadedAt);
        }

        public async Task<GetParticipantUploadsResponse> GetUploads(string? state, int perPage, int page = 0)
        {
            var limit = perPage;
            var offset = perPage * (page - 1);
            var uploads = await _participantUploadDao.GetUploads(state, limit, offset);
            var total = await _participantUploadDao.GetUploadCount(state);

            var meta = _metaBuilder
                .SetPage(page)
                .SetPerPage(perPage)
                .SetState(state)
                .SetTotal(total)
                .Build();

            return new GetParticipantUploadsResponse()
            {
                Data = uploads,
                Meta = meta
            };
        }
    }
}