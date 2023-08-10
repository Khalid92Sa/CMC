using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Controllers;
using CMC.Presentation.Application.Services.OTP;
using CMC.Presentation.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        readonly IOTPService _otpService;
        private Configuration _configuration { set; get; }
        
        public HomeController(ILogger<HomeController> logger, Configuration configuration,IOTPService oTPService)
        {
            _logger = logger;
            _configuration = configuration;
            _otpService = oTPService;
        }

        public async Task<IActionResult> Index()
        {
            bool isOTPEnabled = _configuration.OTPSettings.OTPEnabled;
            var result = await _otpService.Test();
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
