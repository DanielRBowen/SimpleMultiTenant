using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Multitenancy;
using SimpleMultiTenant.Data;
using SimpleMultiTenant.Models;
using SimpleMultiTenant.Services;

namespace SimpleMultiTenant.Controllers
{
    //[Route("/[controller]/[Action]/{tenant?}")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
        }

        public IActionResult Index()
        {
            var tenant = _httpContextAccessor.HttpContext.GetTenant();
            return View(tenant);
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
