using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Generator;

namespace Test.Controllers
{
    [InjectDependencies]
    public partial class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public IActionResult Index()
        {
            _logger.LogInformation("Automagically generated logger!");
            return View();
        }
    }
}
