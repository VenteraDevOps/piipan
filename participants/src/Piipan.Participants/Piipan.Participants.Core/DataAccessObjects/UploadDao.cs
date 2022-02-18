using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Piipan.Participants.Api.Models;
using Piipan.Participants.Core.Models;
using Piipan.Shared.Database;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public class UploadDao : IUploadDao
    {
        private readonly IDbConnectionFactory<ParticipantsDb> _dbConnectionFactory;

        public UploadDao(IDbConnectionFactory<ParticipantsDb> dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IUpload> GetLatestUpload(string state = null)
        {
            using (var connection = await _dbConnectionFactory.Build(state))
            {
                return await connection
                    .QuerySingleAsync<UploadDbo>(@"
                    SELECT id, created_at, publisher
                    FROM uploads
                    ORDER BY id DESC
                    LIMIT 1");
            }
        }

        public async Task<IUpload> AddUpload()
        {
            using (var connection = await _dbConnectionFactory.Build() as DbConnection)
            {
                await connection.OpenAsync();
                var tx = connection.BeginTransaction();

                await connection.ExecuteAsync(@"
                INSERT INTO uploads (created_at, publisher)
                VALUES (now() at time zone 'utc', current_user)");

                var upload = await connection.QuerySingleAsync<UploadDbo>(@"
                    SELECT id, created_at, publisher
                    FROM uploads
                    ORDER BY id DESC
                    LIMIT 1");

                tx.Commit();

                return upload;
            }
        }
    }
}
