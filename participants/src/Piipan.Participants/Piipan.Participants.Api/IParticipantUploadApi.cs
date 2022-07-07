using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantUploadApi
    {
        Task<UploadDto> GetLatestUpload(string state = null);
        Task<UploadDto> AddUpload(string uploadIdentifier);
        Task<int> UpdateUpload(IUpload upload);
        Task<UploadDto> GetUploadById(string uploadIdentifier);
    }
}
