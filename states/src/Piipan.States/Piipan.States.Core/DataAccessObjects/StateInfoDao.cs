using Piipan.States.Api.Models;
using Piipan.Shared.Database;
using Piipan.States.Core.Models;
using Dapper;

namespace Piipan.States.Core.DataAccessObjects
{
    /// <summary>
    /// This class retreives data from the state_info table in order to grab state email, phone, and region
    /// </summary>
    public class StateInfoDao : IStateInfoDao
    {
        private readonly IDbConnectionFactory<StateInfoDb> _dbConnectionFactory;

        public StateInfoDao(IDbConnectionFactory<StateInfoDb> dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        /// <summary>
        /// Get all state info and searches off of state's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IState> GetStateById(string id)
        {
            try
            {
                using (var connection = await _dbConnectionFactory.Build())
                {
                    return await connection
                        .QuerySingleAsync<StateInfoDbo>(@"
                    SELECT id, state, state_abbreviation, email, phone, region
	                FROM state_info WHERE id =@id
                    ORDER BY id DESC
                    LIMIT 1", new { id = id });
                }
            }
            catch (Exception e)
            {
                return null;
            }
            
        }
        /// <summary>
        /// Get all state info and searches off of state's name
        /// </summary>
        /// <param name="state">State name</param>
        /// <returns>Returns matching StateInfoDbo record</returns>
        public async Task<IState> GetStateByName(string state)
        {
            try
            {
                using (var connection = await _dbConnectionFactory.Build())
                {
                    return await connection
                        .QuerySingleAsync<StateInfoDbo>(@"
                    SELECT id, state, state_abbreviation, email, phone, region
	                FROM state_info WHERE state =@state
                    ORDER BY id DESC
                    LIMIT 1", new { state = state });
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }
        /// <summary>
        /// Get all state info for all states
        /// </summary>
        /// <returns>Enumerable of StateInfoDbo Records</returns>
        public async Task<IEnumerable<IState>> GetStates()
        {
            using (var connection = await _dbConnectionFactory.Build())
            {
                return await connection
                    .QueryAsync<StateInfoDbo>(@"
                    SELECT id, state, state_abbreviation, email, phone, region
	                FROM state_info ORDER BY id ASC");
            }
        }
    }
}
