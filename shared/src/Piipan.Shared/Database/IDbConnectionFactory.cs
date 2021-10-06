using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Shared.Database
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> Build(string database = null);
    }
}