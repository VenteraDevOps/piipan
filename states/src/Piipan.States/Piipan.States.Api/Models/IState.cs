using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.States.Api.Models
{
    public interface IState
    {
        string Id { get; set; }
        string State { get; set; }
        string StateAbbreviation { get; set; }
        string Email { get; set; }
        string Phone { get; set; }
        string Region { get; set; }


    }
}
