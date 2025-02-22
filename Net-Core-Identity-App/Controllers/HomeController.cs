﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Net_Core_Identity_App.Models;
using Net_Core_Identity_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager,signInManager)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated) // kullanici login olmussa true donecek.
            {
                return RedirectToAction("Index", "Member"); // index'ten member'a gitsin.
            }

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
                    if (await userManager.IsLockedOutAsync(user)) // eger kullanici kitli ise? kitli ise true donecek.
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz."); // hernagi bir textbox hedeflenmedigi icin ilk secenek bos. 

                        return View(userlogin);
                    }



                    if (userManager.IsEmailConfirmedAsync(user).Result == false)
                    {
                        ModelState.AddModelError("", "Email adresiniz onaylanmamıştır. Lütfen gelen kutunuzu kontrol ediniz."); // hata herhangi bir textbox'i hedef almayacak, o yuzden ilk alan bos.
                        return View(userlogin); // hata ile birlikte userlogin'e donus yapilacak.
                    }




                    await signInManager.SignOutAsync(); // oncesinde kullanici cikis yapiyor. temiz baslangic. kullanici cookie siliniyor.

                    // PasswordSignInAsync method sayesinde kullanici girisi saglaniyor. 60 gun cookie saklanmasini aktif etmek icin true demek lazim. onu checkbox ile kontrol ediyoruz. 
                    // ikinci false ise fail girislerde kullaniciyi kilitleyip kilitlememeyle alakali. simdilik atif etmiyoruz. false diyoruz. 
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, userlogin.Password, userlogin.RememberMe, false);


                    // result.IsLockedOut kullanici kitli mi degil mi veritabanindan kontrol ediyor. true mu false mu diye
                    // result.IsNotAllowed kullanicinin giris izni var mi yok mu? yanlis giris sayisini asmis mi asmamis mi kontrol ediyor. 
                    if (result.Succeeded) // eger basariliysa giris
                    {
                        await userManager.ResetAccessFailedCountAsync(user); // eger basarili giris yaparsa fail girisleri sifirlanacak.

                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString()); // kullanici basarili giris yaptiktan sonra hangi yetkisiz oldugu sayfadan geliyorsa o sayfaya yonlendirilecek. 
                        }

                        //action    controller
                        return RedirectToAction("Index", "Member");
                    }
                    else // giris basarili degilse sifre yanlissa
                    {
                        await userManager.AccessFailedAsync(user); // kullanicinin basarisiz giris counter'ini 1 arttiracak.

                        int fail = await userManager.GetAccessFailedCountAsync(user); // kullanicinin kac kez basarisiz giris yaptigi. 
                        ModelState.AddModelError("", $" {fail} kez başarısız giriş.");
                        if (fail == 3)
                        {
                            await userManager.SetLockoutEndDateAsync(user, new System.DateTimeOffset(DateTime.Now.AddMinutes(20))); // 20 dakika ileriye kadar kitliyor. 

                            ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 20 dakika süreyle kitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Şifreniz doğru değil.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayıtlı kullanıcı bulunamamıştır.");
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
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user); // verilen user bilgisine gore token olusturuldu. 

                    // ConfirmEmail kullanici linke tikladiginda gidecegi sayfa, Home controller dahilinde gelecek. sonrasi query string
                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    },  protocol: HttpContext.Request.Scheme // http mi https mi?

                    );

                    Helper.EmailConfirmation.SendEmail(link, user.Email);

                    return RedirectToAction("LogIn");
                }
                else
                {
                    AddModelError(result);
                }
            }


            return View(userViewModel); // icine userViewModel yazdik cunku kullanici hatali olursa textbox'lari tekrar doldurmayla ugrasmasin hazir gelsin diye.
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel passwordResetViewModel)
        {
            AppUser user = userManager.FindByEmailAsync(passwordResetViewModel.Email).Result; // resut demek sonuclanincaya kadar bekle, alt satira gecme. boyle bir kullanici var mi yok mu kontrol ediliyor. 
            if (user != null) // kayitli bir kullanici varsa
            {
                string passwordResetToken = userManager.GeneratePasswordResetTokenAsync(user).Result; // user bilgilerinden olusan token olusturuyor. 

                // kullanici linke tikladiginda home controller'daki ResetPasswordConfirm func. calisacak. 
                string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);

                //  www.t4h4.net/Home/ResetPasswordConfirm?userId=sdjfsjf&token=dfjkdjfdjf

                Helper.PasswordReset.PasswordResetSendEmail(passwordResetLink, user.Email);

                ViewBag.status = "success"; // view'e mesaj gonderiyoruz.
                TempData["durum"] = true.ToString();
            }
            else
            {
                ModelState.AddModelError("", "Sistemde kayıtlı email adresi bulunamamıştır.");
            }

            return View(passwordResetViewModel);
        }

        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId; // temp dataya gecici olarak kaydediyoruz. 
            TempData["token"] = token;

            return View();
        }


        [HttpPost]
        // PasswordResetViewModel 'den ayrica email'de geliyor lakin buna ihtiyacimiz yok. o yuzden sadece password icin bind kullaniyoruz. sadece PasswordNew dolacak
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")] PasswordResetViewModel passwordResetViewModel)
        {
            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();

            AppUser user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                IdentityResult result = await userManager.ResetPasswordAsync(user, token, passwordResetViewModel.PasswordNew);

                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user); // kullanicilarin kritik bilgilerinin degisip degismedigini anladigimiz security stamp'i guncelliyoruz. sifre degistiginde baska bi cihazdaki baglantida logine atar bu sayede.

                    ViewBag.status = "success";
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "hata meydana gelmiştir. Lütfen daha sonra tekrar deneyiniz.");
            }
                    
            return View(passwordResetViewModel);
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                ViewBag.status = "Email adresiniz onaylanmıştır. Login ekranından giriş yapabilirsiniz.";
            }
            else
            {
                ViewBag.status = "Bir hata meydana geldi. Lütfen daha sonra tekrar deneyiniz.";
            }
            return View();
        }


        public IActionResult GoogleLogin(string ReturnUrl) // erismeye calistigi sayfaya gonderilebilmesi icin ReturnUrl

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl); // saglayici ismi belirtilmeli, saglayiciyla is bittikten sonra RedirectUrl'e donulecek. 

            return new ChallengeResult("Google", properties);
        }


        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/") // return url gelmezse / anasayfaya yonlenecek. 
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync(); // harici loginle alakali bilgiler gelecek.
                                                                                      
            if (info == null)
            {
                return RedirectToAction("LogIn");
            }
            else
            {
                // SignInResult donuyor. cakisma olmamasi icin namespace belirtmek gerekir. Microsoft.AspNetCore.Identity
                Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true); // isPersistent değeri true varsayilan cookie'ye gore 60 gun tutulacak. 
                // daha once kullanici login olmussa, login islemi gerceklesecek. 

                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }

                // kullanici ilk kez geliyorsa;
                else
                {
                    AppUser user = new AppUser();

                    // Principal uzerinden saglayicidan gelen claimlere ulasabiliyoruz. 
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value; // claim'lerden email olan bulunacak. value'si cekilecek.
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value; // NameIdentifier, saglayicidaki id'yi verecek.

                    // saglayicidan gelen ad ve soyad ile username olusturma islemi
                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name)) // name claim varsa
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

                        // bosluk yerine - ve kucuk. baska isimle cakismamasi icin id'de ekleniyor. (ilk 5) 
                        userName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 5).ToString();

                        user.UserName = userName;
                    }
                    else // eger name claim yoksa kullanici adi email
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    IdentityResult createResult = await userManager.CreateAsync(user);

                    if (createResult.Succeeded) // kullanici basarili bi' sekilde olusmussa
                    {
                        
                        IdentityResult loginResult = await userManager.AddLoginAsync(user, info); // login islemi için, AspNetUserLogins tablosuna kayit ediliyor.


                        if (loginResult.Succeeded)
                        {
                            // await signInManager.SignInAsync(user, true); // giris islemi gerceklesiyor. Kullanicinin isPersisten degeri true cekiliyor. Cooki belirtilen sure kadar aktif. 

                            await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true); // gelen kullanicinin saglayicidan geldigi belli olsun diye ExternalLoginSignInAsync kullanildi. Uye claims'den saglayici araciligiyla geldigi belli olacak kullanicinin. 

                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            AddModelError(loginResult);
                        }
                    }
                    else
                    {
                        AddModelError(createResult); // createResult hatalari
                    }
                }
            }

            // Model state yapisindan hatalari liste seklinde LINQ kullanarak cekme ve CustomError sayfasina model olarak gonderme

            List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

            return View("CustomError", errors);
        }

        public ActionResult CustomError()
        {
            return View();
        }
    }
}
