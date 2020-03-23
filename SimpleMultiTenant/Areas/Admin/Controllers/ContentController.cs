using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleMultiTenant.Security;

namespace SimpleMultiTenant.Areas.Admin.Controllers
{
    [Authorize(Policy = PolicyNames.RequireTenant, Roles = "admin")]
    [Area("Admin")]
    public class ContentController : Controller
    {
        [Authorize(Policy = PolicyNames.RequireTenant, Roles = "admin, mod")]
        public IActionResult Mod()
        {
            return View();
        }
    }
}
