using Microsoft.AspNetCore.Mvc;
using BasicCore7.Data;
using BasicCore7.Services;

namespace BasicCore7.Controllers
{
    public class CultController : Controller
    {
        protected readonly BasicCore7Context _context;
        protected readonly ILogger<CultController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly BasicCore7User _user;

        public CultController(BasicCore7Context context, IHttpContextAccessor httpContextAccessor, ILogger<CultController> logger)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            // Haal de gebruiker op van deze request-afhandeling
            _user = Globals.GetUser(httpContextAccessor.HttpContext.User.Identity.Name);
        }
    }
}
