using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net_Core_Identity_App.Models;

namespace Net_Core_Identity_App.Controllers
{
    public class BaseController : Controller
    {
        protected UserManager<AppUser> userManager { get; } // UserManager DI islemi yapiyoruz.
        protected SignInManager<AppUser> signInManager { get; } // SignInManager DI islemi yapiyoruz.
        protected AppUser CurrentUser => userManager.FindByNameAsync(User.Identity.Name).Result;

        public BaseController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public void AddModelError(IdentityResult result)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description); // hatalar modelstate 'e ekleniyor. 
            }
        }
    }
}
