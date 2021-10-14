using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Services
{
    public interface IMatchEventService
    {
        Task ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState);
    }
}
