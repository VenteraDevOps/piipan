using Piipan.Shared.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.Notifications.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(EmailModel emailDetails);
    }
}
