using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Net_Core_Identity_App.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        // User manager DI işlemi.
        public UserManager<AppUser> userManager { get; set; }

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) // principal kullanicilarin bilgilerini tutuyor. User aslinda arka tarafta ClaimsPrincipal sınıfıdır.
        {
            if (principal != null && principal.Identity.IsAuthenticated) // TransformAsync fonk. her zaman calisir. bu yuzden kullanici uye mi degil mi kontrol etmek gerekir.
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity; // principal.Identity geriye IIdentity donuyor. bu sebeble ClaimsIdentity'ye cast ediyoruz.

                AppUser user = await userManager.FindByNameAsync(identity.Name); // kullaniciyi buluyoruz.

                if (user != null) // kullanici varsa 
                {
                    if (user.City != null) 
                    {
                        if (!principal.HasClaim(c => c.Type == "city")) // kullanicida city ile alakali bir claim var mı kontrolu. yoksa asagiya giriyor. 
                        {
                            // CityClaim adinda claim eklenecek. 

                            Claim CityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal"); // dagitimci internal. 

                            identity.AddClaim(CityClaim);
                        }
                    }
                }
            }
            return principal;
        }
    }
}