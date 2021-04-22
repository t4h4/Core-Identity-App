using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Core_Identity_App.Models;
using Net_Core_Identity_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;

namespace Net_Core_Identity_App.Controllers
{
    [Authorize] // yetkisiz girisi yasakladik. cookie olmadan asla. sadece uyeler girebilir.
    public class MemberController : Controller
    {

        public UserManager<AppUser> userManager { get; } // UserManager DI islemi yapiyoruz.
        public SignInManager<AppUser> signInManager { get; } // SignInManager DI islemi yapiyoruz.


        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }


        public IActionResult Index()
        {
            AppUser user = userManager.FindByNameAsync(User.Identity.Name).Result; // uyeyi buluyor ve getiriyoruz. 

            // UserViewModel userViewModel = new UserViewModel(); // user bilgilerini modele gore aliyoruz. 
            // userViewModel.UserName = user.UserName;

            // yukaridaki kod fazlaligi. bunun olmamasi icin mapster kutuphanesini kullaniyoruz. mapster AppUser modelden UserViewModel'e otomatik donus saglayacak. 
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel); // userViewModel dolmus olarak geldi. bunu view'e gonderiyoruz.
        }
    }
}
