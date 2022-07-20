using Piipan.States.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.States.Core.DataAccessObjects
{
    public interface IStateInfoDao
    {
        Task<IState> GetStateByName(string state);
        Task<IState> GetStateById(string id);
        Task<IEnumerable<IState>> GetStates();
    }
}
