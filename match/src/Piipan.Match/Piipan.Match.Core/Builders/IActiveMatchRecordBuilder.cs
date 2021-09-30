using Piipan.Match.Api;
using Piipan.Match.Orchestrator;

namespace Piipan.Match.Core.Builders
{
    public interface IActiveMatchRecordBuilder
    {
        IActiveMatchRecordBuilder SetMatch(RequestPerson input, ParticipantRecord match);
        IActiveMatchRecordBuilder SetStates(string stateA, string stateB);

        IMatchRecord GetRecord();
    }
}
