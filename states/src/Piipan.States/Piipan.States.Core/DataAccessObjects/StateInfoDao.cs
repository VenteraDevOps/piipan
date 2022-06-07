using Piipan.States.Api.Models;
using Piipan.Shared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Piipan.States.Core.Models;
using Dapper;

namespace Piipan.States.Core.DataAccessObjects
{
    public class StateInfoDao : IStateInfoDao
    {
        private readonly IDbConnectionFactory<StateInfoDb> _dbConnectionFactory;

        public StateInfoDao(IDbConnectionFactory<StateInfoDb> dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IState> GetStateById(string id)
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

        public async Task<IState> GetStateByName(string state)
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
    }
}
