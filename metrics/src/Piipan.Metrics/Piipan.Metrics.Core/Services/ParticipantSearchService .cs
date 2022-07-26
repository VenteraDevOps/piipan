using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Core.Services
{
    /// <summary>
    /// Service layer for creating Metrics Participant Search records
    /// </summary>
    public class ParticipantSearchService : IParticipantSearchWriterApi
    {
        private readonly IParticipantSearchDao _participantSearchDao;
        private readonly IMetaBuilder _metaBuilder;

        public ParticipantSearchService(IParticipantSearchDao commonMetricsDao, IMetaBuilder metaBuilder)
        {
            _participantSearchDao = commonMetricsDao;
            _metaBuilder = metaBuilder;
        }

        public async Task<int> AddSearchMetrics(ParticipantSearch newParticipantSearch)
        {
            return await _participantSearchDao.AddParticipantSearchRecord(new ParticipantSearchDbo(newParticipantSearch));
        }

    }
}
