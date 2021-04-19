using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Net_Core_Identity_App.Models;
using Net_Core_Identity_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public UserManager<AppUser> userManager { get; } // UserManager DI islemi yapiyoruz.
        public SignInManager<AppUser> signInManager { get; } // SignInManager DI islemi yapiyoruz.

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize] // yetkisiz girisi yasakladik. cookie olmadan asla. 
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public IActionResult LogIn(string ReturnUrl) // return url yetkisiz sayfaya girince direk yukardaki urlden geliyor. yetkisiz girisler cookie'den login'e aktarildigi icin burada bu isi yapiyoruz. 
        {
            TempData["ReturnUrl"] = ReturnUrl; // action'lar arasi veri aktarmak icin TempData kullanmak ideal. 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel userlogin)
        {
            if (ModelState.IsValid) // view'de girilen model validasyonu gecerliyse, dogruysa
            {
                AppUser user = await userManager.FindByEmailAsync(userlogin.Email); // kullanici var mi yok mu?

                if (user != null)
                {
                    await signInManager.SignOutAsync(); // oncesinde kullanici cikis yapiyor. temiz baslangic. kullanici cookie siliniyor.

                    // PasswordSignInAsync method sayesinde kullanici girisi saglaniyor. 60 gun cookie saklanmasini aktif etmek icin true demek lazim. onu checkbox ile kontrol ediyoruz. 
                    // ikinci false ise fail girislerde kullaniciyi kilitleyip kilitlememeyle alakali. simdilik atif etmiyoruz. false diyoruz. 
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, userlogin.Password, userlogin.RememberMe, false);


                    // result.IsLockedOut kullanici kitli mi degil mi veritabanindan kontrol ediyor. true mu false mu diye
                    // result.IsNotAllowed kullanicinin giris izni var mi yok mu? yanlis giris sayisini asmis mi asmamis mi kontrol ediyor. 
                    if (result.Succeeded) // eger basariliysa giris
                    {
                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString()); // kullanici basarili giris yaptiktan sonra hangi yetkisiz oldugu sayfadan geliyorsa o sayfaya yonlendirilecek. 
                        }

                        //action    controller
                        return RedirectToAction("Index", "Member");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz email adresi veya şifresi.");
                }
            }
            return View(userlogin); // onceki commitlerde unutuldu yeni yapildi. hata olursa gene ayni sekilde dolsun diye. 
        }



        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            if(ModelState.IsValid) // view'de girilen model validasyonu gecerliyse, dogruysa
            {
                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;

                // IdentityResult result olarak degiskene atiyoruz ki daha fazla detaylara bu degisken sayesinde ulasabilelim.
                IdentityResult result = await userManager.CreateAsync(user, userViewModel.Password); // pass hash islemi yapilacagi icin burda. 

                if(result.Succeeded) // eger kullanici basarili sekilde kaydolduysa onu login ekranina yonlendirecegiz. 
                {
                    return RedirectToAction("LogIn");
                }
                else
                {
                    foreach (IdentityError item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                    }
                }
            }


            return View(userViewModel); // icine userViewModel yazdik cunku kullanici hatali olursa textbox'lari tekrar doldurmayla ugrasmasin hazir gelsin diye.
        }

    }
}
