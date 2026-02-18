using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cybarch.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Zjednodušenie UI: Home/Dashboard redirectuje na Universes.
        return RedirectToAction("Index", "Universes");
    }
}
