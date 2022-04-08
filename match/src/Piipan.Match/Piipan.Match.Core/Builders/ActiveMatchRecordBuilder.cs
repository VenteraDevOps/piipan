using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;
using Newtonsoft.Json;

namespace Piipan.Match.Core.Builders
{
    /// <summary>
    /// Builder for creating IMatchRecord objects from match events data
    /// </summary>
    public class ActiveMatchRecordBuilder : IActiveMatchRecordBuilder
    {
        private MatchRecordDbo _record = new MatchRecordDbo();

        /// <summary>
        /// Initializes a new instance of ActiveMatchRecordBuilder
        /// </summary>
        public ActiveMatchRecordBuilder()
        {
            this.Reset();
        }

        /// <summary>
        /// Reset the builder's internal record reference
        /// </summary>
        public void Reset()
        {
            this._record = new MatchRecordDbo();
        }

        /// <summary>
        /// Set the match record's match-related fields (Input, Data, Hash, HashType)
        /// </summary>
        /// <remarks>
        /// Currently only supports the "ldshash" hash type.
        /// </remarks>
        /// <param name="input">The RequestPerson object received as input to the active match API request.</param>
        /// <param name="match">The ParticipantRecord object received as output from active match API response.</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public IActiveMatchRecordBuilder SetMatch(RequestPerson input, IParticipant match)
        {
            this._record.Input = JsonConvert.SerializeObject(input);
            this._record.Data = JsonConvert.SerializeObject(match);

            // ldshash is currently the only hash type
            this._record.Hash = input.LdsHash;
            this._record.HashType = "ldshash";

            return this;
        }

        /// <summary>
        /// Set the match record's state-related fields (Initiator, States[])
        /// </summary>
        /// <param name="initiatingState">The two-letter postal abbreviation of the initiating state.</param>
        /// <param name="matchingState">The two-letter postal abbreviation of the matching state.</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public IActiveMatchRecordBuilder SetStates(string initiatingState, string matchingState)
        {
            this._record.States = new string[] { initiatingState, matchingState };
            this._record.Initiator = initiatingState;
            return this;
        }

        /// <summary>
        /// Get the built record and reset internal record reference.
        /// </summary>
        /// <param name="initiatingState">The two-letter postal abbreviation of the initiating state.</param>
        /// <param name="matchingState">The two-letter postal abbreviation of the matching state.</param>
        /// <returns>Current IMatchRecord instance</returns>
        public IMatchRecord GetRecord()
        {
            MatchRecordDbo record = this._record;

            this.Reset();

            return record;
        }
    }
}
