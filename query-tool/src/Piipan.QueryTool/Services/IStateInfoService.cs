using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.QueryTool.Services
{
    public interface IStateInfoService
    {
        public Task<StateInfoResponse> GetStateInfoAsync();
    }
}
