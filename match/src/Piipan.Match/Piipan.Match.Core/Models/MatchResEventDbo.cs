using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;

#nullable enable

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Implementation of IMatchResEvent for database interactions
    /// </summary>
    public class MatchResEventDbo : IMatchResEvent
    {
        public int? Id { get; set; }
        public string? MatchId { get; set; }
        public DateTime? InsertedAt { get; set; }
        public string? Actor { get; set; }
        public string? ActorState { get; set; }
        public string? Delta { get; set; }

    }
}
