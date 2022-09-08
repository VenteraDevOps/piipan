using System;
using Microsoft.Extensions.Logging;

namespace Piipan.Dashboard.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
