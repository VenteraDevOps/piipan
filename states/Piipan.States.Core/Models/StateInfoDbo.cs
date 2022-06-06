using Piipan.States.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.States.Core.Models
{
    public class StateInfoDbo : IState
    {
        public string? Id { get; set; }
        public string State { get; set; }
        public string StateAbbreviation { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Region { get; set; }
    }
}
