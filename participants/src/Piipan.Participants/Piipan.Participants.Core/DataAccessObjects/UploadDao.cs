using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;
using Piipan.Participants.Core.Enums;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class UploadDao : IUploadDao
    {
        private readonly IDbConnectionFactory<ParticipantsDb> _dbConnectionFactory;

        public UploadDao(IDbConnectionFactory<ParticipantsDb> dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Retrieves the most recent successful upload record
        /// </summary>
        /// <param name="state">The State we want to retrieve the latest upload for</param>
        /// <returns>The latest successful Upload record in the database</returns>
        public async Task<IUpload> GetLatestUpload(string state = null)
        {
            using (var connection = await _dbConnectionFactory.Build(state))
            {
                return await connection
                    .QuerySingleAsync<UploadDbo>(@"
                    SELECT 
                        id Id, 
                        created_at CreatedAt, 
                        publisher Publisher,
                        upload_identifier UploadIdentifier, 
                        status Status, 
                        completed_at CompletedAt, 
                        participants_uploaded ParticipantsUploaded, 
                        error_message ErrorMessage
                    FROM uploads 
                    where status=@completeStatus
                    ORDER BY id DESC
                    LIMIT 1", new { completeStatus = UploadStatuses.COMPLETE.ToString() });
            }
        }

        /// <summary>
        /// Adds a new upload record to the database.
        /// </summary>
        /// <param name="uploadIdentifier">The unique identifier of the upload record to be added to the database </param>
        /// <returns>The Upload record that was created in the database</returns>
        public async Task<IUpload> AddUpload(string uploadIdentifier)
        {
            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                await connection.OpenAsync();
               
                await connection.ExecuteAsync(@"
                INSERT INTO uploads (created_at, publisher,upload_identifier, status)
                VALUES (now() at time zone 'utc', current_user,@uploadIdentifier, @uploadStatus)", new { uploadIdentifier = uploadIdentifier , uploadStatus = UploadStatuses.UPLOADING.ToString()});

                var upload = await connection.QuerySingleAsync<UploadDbo>(@"
                    SELECT 
                        id Id, 
                        created_at CreatedAt, 
                        publisher Publisher,
                        upload_identifier UploadIdentifier, 
                        status Status, 
                        completed_at CompletedAt, 
                        participants_uploaded ParticipantsUploaded, 
                        error_message ErrorMessage
                    FROM uploads 
                    ORDER BY id DESC
                    LIMIT 1");

                return upload;
            }
        }

        /// <summary>
        /// Retrieves upload record by id
        /// </summary>
        /// <param name="uploadIdentifier">The upload id of the desired record</param>
        /// <returns>Upload Record whose upload_identifier matches uploadIdentifier</returns>
        public async Task<IUpload> GetUploadById(string uploadIdentifier)
        {
            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                await connection.OpenAsync();

                return await connection
                    .QuerySingleAsync<UploadDbo>(@"
                    SELECT 
                        id Id, 
                        created_at CreatedAt, 
                        publisher Publisher,
                        upload_identifier UploadIdentifier, 
                        status Status, 
                        completed_at CompletedAt, 
                        participants_uploaded ParticipantsUploaded, 
                        error_message ErrorMessage
                    FROM uploads 
                    where upload_identifier=@uploadIdentifier
                    ORDER BY id DESC
                    LIMIT 1", new { uploadIdentifier = uploadIdentifier });
            }
        }

        /// <summary>
        /// Updates a upload record in the database.
        /// </summary>
        /// <param name="uploadDbo">The Upload values to be used in updating that Upload record in the database</param>
        /// <returns>Number of Upload records that were updated</returns>
        public async Task<int> UpdateUpload(IUpload uploadDbo)
        {
            using (var connection = await _dbConnectionFactory.Build(null))
            {
                return await connection.ExecuteAsync(@"
                    UPDATE uploads 
                    SET publisher=@publisher, created_at=@created_at, completed_at=@completed_at, status=@status, 
                        participants_uploaded=@participants_uploaded, error_message=@error_message
                    WHERE upload_identifier=@upload_identifier;",
                    new
                    {
                        publisher = uploadDbo.Publisher,
                        created_at = uploadDbo.CreatedAt,
                        completed_at = uploadDbo.CompletedAt,
                        status = uploadDbo.Status,
                        upload_identifier = uploadDbo.UploadIdentifier,
                        participants_uploaded = uploadDbo.ParticipantsUploaded,
                        error_message = uploadDbo.ErrorMessage
                    });
            }
        }
    }
}
