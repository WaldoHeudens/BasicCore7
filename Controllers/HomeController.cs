using BasicCore7.Data;
using BasicCore7.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BasicCore7.Controllers
{
    public class HomeController : CultController
    {

        public HomeController(BasicCore7Context context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CultController> logger)
            : base(context, httpContextAccessor, logger)
        {
        }

            public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}