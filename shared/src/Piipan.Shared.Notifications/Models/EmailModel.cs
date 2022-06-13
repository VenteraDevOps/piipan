using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.Notifications.Models
{
    public class EmailModel
    {
        public List<string> ToList { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string Body { get; set; }


        public EmailModel()
        {

        }
    }
}
