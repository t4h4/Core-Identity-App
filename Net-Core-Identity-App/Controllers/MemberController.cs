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
using Microsoft.AspNetCore.Mvc.Rendering;
using Net_Core_Identity_App.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Net_Core_Identity_App.Controllers
{
    [Authorize] // yetkisiz girisi yasakladik. cookie olmadan asla. sadece uyeler girebilir.
    public class MemberController : BaseController
    {

        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(userManager,signInManager)
        {
    
        }


        public IActionResult Index()
        {
            AppUser user = CurrentUser; // uyeyi buluyor ve getiriyoruz. Base controller yardimiyla.

            // UserViewModel userViewModel = new UserViewModel(); // user bilgilerini modele gore aliyoruz. 
            // userViewModel.UserName = user.UserName;

            // yukaridaki kod fazlaligi. bunun olmamasi icin mapster kutuphanesini kullaniyoruz. mapster AppUser modelden UserViewModel'e otomatik donus saglayacak. 
            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            return View(userViewModel); // userViewModel dolmus olarak geldi. bunu view'e gonderiyoruz.
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser; // kullaniciyi elde ediyoruz. name ile buluyoruz. user identity kesin gelir cunku bu sinif login olanlarin girebilecegi authorize ayarlanmis bir sinif. base controller yardimiyla

            UserViewModel userViewModel = user.Adapt<UserViewModel>(); // // elde ettigimiz user'i cast ediyoruz. daha once olusturdugumuz userview modele cast ediyoruz. yani o modeldeki property alanlarini degistirebilir sadece. 

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(userViewModel); // view'e dolu bir userViewModel'i veriyoruz. 
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel, IFormFile userPicture) // UserEdit.cshtml sayfasindan userViewModel gelecek. ayrica sayfadan userPicture'i da aldik.
        {
           ModelState.Remove("Password"); // password alani kullanmadigimiz icin kaldirmamiz gerekir. yoksa asagidaki valid'e takilir.
           ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            if (ModelState.IsValid) // gelen bilgiler validse
            {
                AppUser user = CurrentUser; // base controller yardimiyla.


                if (userPicture != null && userPicture.Length > 0) // user picture bos degil ve dolu ise
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName); // Guid.NewGuid().ToString() rastgele bi' isim olusturuyor. Path.GetExtension(userPicture.FileName) sonuna dosya uzantisini ekliyor. 15asd15as1d5.png gibi. 

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", fileName); // resmi kayit edecegimiz yer. 

                    using (var stream = new FileStream(path, FileMode.Create)) // FileMode.Create, path bossa olustur demek. 
                    {
                        await userPicture.CopyToAsync(stream); // kayit yoluna resmi kopyaliyoruz. 

                        user.Picture = "/UserPicture/" + fileName; // veritabanina kayit yolu kaydedildi. 
                    }
                }


                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;

                user.BirthDay = userViewModel.BirthDay;

                user.Gender = (int)userViewModel.Gender; // database'de int oldugu icin enum'in int'e cast edilmesi lazim. 

                IdentityResult result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                    await signInManager.SignOutAsync();
                    await signInManager.SignInAsync(user, true);

                    ViewBag.success = "true"; // flag
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(userViewModel); // view'e tekrar gonderiyoruz. gondermezsek kullanici hata alirsa textbox icleri bos olur. 
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
                AppUser user = CurrentUser; // name degerini o anki cookie bilgisinden okuyor. veritabanindan gelmiyor. async olmadan direk result'tan gidildi. base controller yardimiyla.

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
                        AddModelError(result);
                    }
                }
                else // eski sifrenin yanlis olma durumu.  exist'e gore. 
                {
                    ModelState.AddModelError("", "Eski şifreniz yanlış");
                }
            }

            return View(passwordChangeViewModel); // hatalar varsa hatalari tekrar gosterebilmek icin kullaniciya gonderiyoruz. ilgili alanlar textboxlar tekrar dolsun. 
        }

        public void LogOut()
        {
            signInManager.SignOutAsync();
        }

    }
}
