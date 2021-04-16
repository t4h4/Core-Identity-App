using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Core_Identity_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> userManager { get; }
        // DI sayesinde UserManager'dan nesne alabilir ve onun içini doldurabiliriz. 
        public AdminController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(userManager.Users.ToList());
        }
    }
}
