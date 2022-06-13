using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Piipan.QueryTool.Pages
{
    public class NotAuthorizedModel : BasePageModel
    {
        public string Message = "";

        public NotAuthorizedModel(IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
        }

        public IActionResult OnGet()
        {
            Message = "You do not have a sufficient role or location to access this page";
            return new PageResult { StatusCode = 403 };

        }
    }
}
