using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Multitenancy;
using SimpleMultiTenant.Data;
using SimpleMultiTenant.Extensions;
using SimpleMultiTenant.Models;
using SimpleMultiTenant.Services;
using System;
using System.Diagnostics;

namespace SimpleMultiTenant.Controllers
{
    //[Route("/[controller]/[Action]/{tenant?}")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IDistributedCache _distributedCache;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext applicationDbContext,
            IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _distributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            var tenant = _httpContextAccessor.HttpContext.GetTenant();
            var cacheJunk = _distributedCache.Get($"CacheJunk{tenant.Name}").FromByteArray<Junk>();

            if (cacheJunk == null)
            {
                var newJunk = new Junk
                {
                    Id = 1,
                    Name = "Moldy Smelly clothes",
                    Description = "Clothes your gradma or grandpa probably wore.",
                    Owner = tenant.Name
                };

                _distributedCache.Set($"CacheJunk{tenant.Name}", newJunk.ToByteArray());
            }

            var viewModel = new HomeIndexViewModel
            {
                Tenant = tenant,
                Junk = cacheJunk
            };

            return View(viewModel);
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

    [Serializable]
    public class Junk
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Owner { get; set; }
    }

    public class HomeIndexViewModel
    {
        public Tenant Tenant { get; set; }

        public Junk Junk { get; set; }
    }
}
