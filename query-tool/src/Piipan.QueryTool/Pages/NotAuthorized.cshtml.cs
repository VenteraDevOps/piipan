using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piipan.Shared.Authorization;
using System;

namespace Piipan.QueryTool.Pages
{
    public class NotAuthorizedModel : BasePageModel
    {
        public string Message = "";

        public NotAuthorizedModel(IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
        }

        [IgnoreAuthorization]
        public IActionResult OnGet()
        {
            Message = "You do not have a sufficient role or location to access this page";
            return new PageResult { StatusCode = 403 };

        }
    }
}
