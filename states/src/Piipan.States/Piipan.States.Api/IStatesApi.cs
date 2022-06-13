using Piipan.States.Api.Models;

namespace Piipan.States.Api
{
    public interface IStatesApi
    {
        Task<StatesInfoResponse> GetStates();
    }
}
