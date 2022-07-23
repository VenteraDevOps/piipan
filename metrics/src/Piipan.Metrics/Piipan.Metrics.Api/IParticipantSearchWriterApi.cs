using System;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public interface IParticipantSearchWriterApi
    {
        Task<int> AddSearchMetrics(ParticipantSearch newParticipantSearch);
    }
}