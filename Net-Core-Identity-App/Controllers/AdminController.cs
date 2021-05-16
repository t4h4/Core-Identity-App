using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Core_Identity_App.Models;
using Net_Core_Identity_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager) // signInManager istiyor lakin burada kullanmaya gerek yok o yuzden null giriyoruz.
        {
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel roleViewModel) // kullanici submit butonuna bastiginda roleViewModel gelecek.
        {
            AppRole role = new AppRole(); // Model icerisindeki AppRole'dan role olusturuyoruz.
            role.Name = roleViewModel.Name;
            IdentityResult result = roleManager.CreateAsync(role).Result; // rol kayit islemi gerceklesiyor. donus IdentityResult tipinde result degiskeninde.

            if (result.Succeeded)

            {
                return RedirectToAction("Roles"); // kayit basariliysa rollerin listelendigi sayfaya gidilecek.
            }
            else
            {
                AddModelError(result);
            }

            return View(roleViewModel);
        }

        public IActionResult Roles()
        {
            return View(roleManager.Roles.ToList()); // Rollerin listelendigi sayfaya rolleri gonderiyoruz. 
        }


        public IActionResult Users()
        {
            return View(userManager.Users.ToList());
        }
    }
}
