using Piipan.Metrics.Api;
using System.Threading.Tasks;

namespace Piipan.Match.Core.Services
{
    public interface IParticipantPublishSearchMetric
    {
        Task PublishSearchdMetric(ParticipantSearchMetrics metrics);
    }
}
