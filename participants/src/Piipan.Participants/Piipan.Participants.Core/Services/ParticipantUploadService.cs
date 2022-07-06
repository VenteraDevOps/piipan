using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.Services
{
    /// <summary>
    /// Service layer for managing participant upload metadata
    /// </summary>
    public class ParticipantUploadService : IParticipantUploadApi
    {
        private readonly IUploadDao _uploadDao;
        private readonly ILogger<ParticipantUploadService> _logger;
        public ParticipantUploadService(
            IUploadDao uploadDao, ILogger<ParticipantUploadService> logger)
        {
            _uploadDao = uploadDao;
            _logger = logger;
        }

        /// <summary>
        /// Adds metadata for a new upload.
        /// </summary>
        /// <param name="uploadIdentifier">The unique identifier of the upload to be added </param>
        /// <returns>The Upload record that was created in the database</returns>
        public async Task<IUpload> AddUpload(string uploadIdentifier)
        {
            var upload = await _uploadDao.AddUpload(uploadIdentifier);
            return new UploadDto(upload);
        }

        /// <summary>
        /// Retrieves the metadata for the most recent successful upload
        /// </summary>
        /// <param name="state">The State we want to retrieve the latest upload for</param>
        /// <returns>The latest successful Upload</returns>
        public async Task<IUpload> GetLatestUpload(string state = null)
        {
            var upload = await _uploadDao.GetLatestUpload(state);
            return new UploadDto(upload);
        }

        /// <summary>
        /// Retrieves upload metadata by id
        /// </summary>
        /// <param name="uploadIdentifier">The desired upload id for the upload we want to retrieve metadata</param>
        /// <returns>Upload whose upload identifier matches uploadIdentifier</returns>
        public async Task<IUpload> GetUploadById(string uploadIdentifier)
        {
            var upload = await _uploadDao.GetUploadById(uploadIdentifier);
            return new UploadDto(upload);
        }

        /// <summary>
        /// Update metadata for an upload.
        /// </summary>
        /// <param name="uploadDbo">The Upload metadata to update</param>
        /// <returns>Number of Uploads that were updated</returns>
        public async Task<int> UpdateUpload(IUpload upload)
        {
            return await _uploadDao.UpdateUpload(upload);
        }
    }
}
