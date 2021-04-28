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

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {
            if (ModelState.IsValid) // view'de girilen model validasyonu gecerliyse, dogruysa
            {
                AppUser user = userManager.FindByNameAsync(User.Identity.Name).Result; // name degerini o anki cookie bilgisinden okuyor. veritabanindan gelmiyor. async olmadan direk result'tan gidildi.

                bool exist = userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result; // girilen eski sifrenin dogrulugu kontrol ediliyor. 

                if (exist)
                {
                    IdentityResult result = userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result; // sifre degistirme islemi yapiliyor. 

                    if (result.Succeeded) // eger basariyla sifre degismisse 
                    {
                        userManager.UpdateSecurityStampAsync(user); // SecurityStamp guncelle. 
                        // eger asagidaki iki parcayi girmeseydik identity api otomatik 30 dakika sonra oturumu sonlandiracakti, login sayfasina yonlendirecekti.  bu varsayilan bi' ayardi. biz hemen cikis ve giris yaptik. 
                        signInManager.SignOutAsync(); // sifre degistigi icin cikis islemi yapiliyor. 
                        signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false); // kullanici giris islemi saglaniyor. true ise cookie'nin startup.cs de belirlenen gecerlilik suresi aktif ediliyor. false ile kilitleme islemi deaktif. 

                        // ornegin hem kullanici hem bayi login ekranlari varsa bunlarin secimi icin signInManager.SignInAsync() method kullanilir. (ekstra durum)

                        ViewBag.success = "true"; // flag atiliyor. 
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description); // hatalar modelstate 'e ekleniyor. 
                        }
                    }
                }
                else // eski sifrenin yanlis olma durumu.  exist'e gore. 
                {
                    ModelState.AddModelError("", "Eski şifreniz yanlış");
                }
            }

            return View(passwordChangeViewModel); // hatalar varsa hatalari tekrar gosterebilmek icin kullaniciya gonderiyoruz. ilgili alanlar textboxlar tekrar dolsun. 
        }
    }
}
