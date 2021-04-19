using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Controllers
{
    [Authorize] // yetkisiz girisi yasakladik. cookie olmadan asla. 
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
