using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantUploadApi
    {
        Task<IUpload> GetLatestUpload(string state = null);
        Task<IUpload> AddUpload(string uploadIdentifier);
        Task<int> UpdateUpload(IUpload upload);
        Task<IUpload> GetUploadById(string uploadIdentifier);
    }
}
