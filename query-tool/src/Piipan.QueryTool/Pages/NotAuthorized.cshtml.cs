using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Piipan.QueryTool.Pages
{
    [AllowAnonymous]
    public class NotAuthorizedModel : BasePageModel
    {
        public string Message = "";
        public RenderMode RenderMode { get; set; } = RenderMode.Static;

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
