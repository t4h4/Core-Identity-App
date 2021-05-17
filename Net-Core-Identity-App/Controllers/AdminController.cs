using Mapster;
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

        public IActionResult RoleDelete(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;
            if (role != null)
            {
                IdentityResult result = roleManager.DeleteAsync(role).Result; // Geri donus tipi IdentityResult.
            }

            return RedirectToAction("Roles");
        }

        public IActionResult RoleUpdate(string id)
        {
            AppRole role = roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                return View(role.Adapt<RoleViewModel>()); // mapster yardimiyla ayni isimlere sahip propertyler otomatik eslenecek. IdentityRole'de de name ve id var, RoleViewModel'de de name ve id var esleniyor. esliyoruz cunku olusturdugumuz guncelleme sayfasi RoleViewModel yapisiyla olusturulacak.
            }

            return RedirectToAction("Roles");
        }

        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            AppRole role = roleManager.FindByIdAsync(roleViewModel.Id).Result;

            if (role != null)
            {
                role.Name = roleViewModel.Name;
                IdentityResult result = roleManager.UpdateAsync(role).Result;

                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme işlemi başarısız oldu.");
            }

            return View(roleViewModel);
        }


        public IActionResult RoleAssign(string id)
        {
            TempData["userId"] = id;
            AppUser user = userManager.FindByIdAsync(id).Result;

            ViewBag.userName = user.UserName;

            IQueryable<AppRole> roles = roleManager.Roles; // roller database'den cekiliyor. cunku checkbox uzerinde gosterilecek.

            List<string> userroles = userManager.GetRolesAsync(user).Result as List<string>; // user'in sahip oldugu roller liste icine donuyor. .result sonucu Ilist oldugu icin as List diyerek cast ettik. 

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>(); // RoleAssignViewModel yapisinda bir bos liste olusturuyoruz.

            foreach (var role in roles) // database'de var olan rolleri donuyoruz. 
            {
                RoleAssignViewModel r = new RoleAssignViewModel();
                r.RoleId = role.Id;
                r.RoleName = role.Name;
                if (userroles.Contains(role.Name)) // o sirada donulen rol, secilen kullanicida var mi yok mu?
                {
                    r.Exist = true; // kullanici role sahip, checkbox isaretli.
                }
                else
                {
                    r.Exist = false; // checkbox bos. 
                }
                roleAssignViewModels.Add(r); // list'e ekleme yap.
            }

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> roleAssignViewModels)
        {
            AppUser user = userManager.FindByIdAsync(TempData["userId"].ToString()).Result; // get fonksiyonda su sekilde aldiydik = TempData["userId"] = id;

            foreach (var item in roleAssignViewModels)
            {
                if (item.Exist) 

                {
                    await userManager.AddToRoleAsync(user, item.RoleName); // isaretli olan checkbox rolu, kullaniciya ata.
                }
                else
                {
                    await userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("Users"); // rol atama, kaldirma tamamlandiktan sonra kullanici, uyeler sayfasina atiliyor.
        }

    }
}
