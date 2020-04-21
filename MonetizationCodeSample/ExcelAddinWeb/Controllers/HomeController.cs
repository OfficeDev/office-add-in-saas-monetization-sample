namespace ExcelAddInWeb.Controllers
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc;
    using ExcelAddInWeb.Models;

    public class HomeController : Controller
    {
        public IActionResult AddInIndex()
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
