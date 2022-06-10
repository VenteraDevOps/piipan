using Piipan.States.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.States.Core.Models
{
    /// <summary>
    /// Implementation of IState for database interactions
    /// </summary>
    public class StateInfoDbo : IState
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string StateAbbreviation { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Region { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            StateInfoDbo p = obj as StateInfoDbo;
            if (p == null)
            {
                return false;
            }

            return
                Id == p.Id &&
                State == p.State &&
                StateAbbreviation == p.StateAbbreviation &&
                Email == p.Email &&
                Phone == p.Phone &&
                Region == p.Region;
        }
    }
}
