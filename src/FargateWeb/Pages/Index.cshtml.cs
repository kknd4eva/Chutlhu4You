using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FargateWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // grab the users ip address and store it in session
            _logger.LogInformation("visit from IP: {IpAddress}", HttpContext?.Connection?.RemoteIpAddress?.ToString());
            HttpContext.Session.SetString("IpAddress", HttpContext.Connection.RemoteIpAddress.ToString());

        }
    }
}
