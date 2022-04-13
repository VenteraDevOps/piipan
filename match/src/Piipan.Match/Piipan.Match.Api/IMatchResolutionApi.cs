using Piipan.Match.Api.Models.Resolution;
using System.Threading.Tasks;

namespace Piipan.Match.Api
{
    public interface IMatchResolutionApi
    {
        Task<MatchResApiResponse> GetMatch(string matchId);
    }
}
