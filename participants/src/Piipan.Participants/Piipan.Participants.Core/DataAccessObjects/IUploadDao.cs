
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public interface IUploadDao
    {
        Task<IUpload> GetLatestUpload(string state = null);
        Task<IUpload> AddUpload(string uploadIdentifier);
        Task<IUpload> UpdateUploadStatus(IUpload upload, string newStatus);
    }
} 