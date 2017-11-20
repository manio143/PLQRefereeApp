using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace PLQRefereeApp
{
    public class DirectoryController : Controller
    {
        private UserRepository UserRepository { get; }
        public DirectoryController(UserRepository userRepository)
        {
            UserRepository = userRepository;
        }

        [Route("/directory")]
        [HttpGet]
        public IActionResult Directory(string filter = "all")
        {
            var refs = UserRepository.GetAllUserData();

            if (filter.Equals("any", StringComparison.InvariantCultureIgnoreCase))
                refs = refs.Where(ud => ud.IsCertified);
            else if (filter.Equals("AR", StringComparison.InvariantCultureIgnoreCase))
                refs = refs.Where(ud => ud.IsArCertified);
            else if (filter.Equals("SR", StringComparison.InvariantCultureIgnoreCase))
                refs = refs.Where(ud => ud.IsSrCertified);
            else if (filter.Equals("HR", StringComparison.InvariantCultureIgnoreCase))
                refs = refs.Where(ud => ud.IsHrCertified);

            return View(refs);
        }
    }
}