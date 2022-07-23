using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Models;

#nullable enable

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantSearchDao
    {
       Task<int> AddParticipantSearchRecord(ParticipantSearchDbo newSearchDbo);
    }
}