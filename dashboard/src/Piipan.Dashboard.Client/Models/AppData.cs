using Piipan.States.Api.Models;

namespace Piipan.Dashboard.Client.Models
{
    public record AppData
    {
        public StatesInfoResponse StateInfo { get; set; }
    }
}
