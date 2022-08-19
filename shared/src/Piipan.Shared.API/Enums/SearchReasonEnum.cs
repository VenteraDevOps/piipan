using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.API.Enums
{
    public enum ValidSearchReasons
    {
        [Display(Name = "New Application")]
        application,
        [Display(Name = "Recertification")] 
        recertification,
        [Display(Name = "New Household Member")] 
        new_household_member,
        [Display(Name = "Other")] 
        other
    }
}
