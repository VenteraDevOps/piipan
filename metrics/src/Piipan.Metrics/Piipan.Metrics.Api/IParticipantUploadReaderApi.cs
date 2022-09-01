using System.Threading.Tasks;

#nullable enable

namespace Piipan.Metrics.Api
{
    public interface IParticipantUploadReaderApi
    {
        Task<GetParticipantUploadsResponse> GetLatestUploadsByState();
        Task<GetParticipantUploadsResponse> GetUploads(ParticipantUploadRequestFilter filter);
    }
}