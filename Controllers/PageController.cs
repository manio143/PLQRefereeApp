using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class PageController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index() {
            return View();
        }

        [HttpGet("/materials")]
        public IActionResult Materials() {
            return View();
        }

        [HttpGet("/payment")]
        public IActionResult Payment() {
            return View();
        }
    }
}
