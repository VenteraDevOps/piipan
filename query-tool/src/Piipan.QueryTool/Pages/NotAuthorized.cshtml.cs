using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Piipan.QueryTool.Pages
{
    [AllowAnonymous]
    public class NotAuthorizedModel : BasePageModel
    {
        public string Message = "";

        public NotAuthorizedModel(IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
        }

        public IActionResult OnGet()
        {
            return new PageResult { StatusCode = 403 };
        }
    }
}
