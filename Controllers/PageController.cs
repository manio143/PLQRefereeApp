using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class PageController : Controller
    {
        [Route("/")]
        public IActionResult Index() {
            return View();
        }

        [Route("/materials")]
        public IActionResult Materials() {
            return View();
        }
    }
}