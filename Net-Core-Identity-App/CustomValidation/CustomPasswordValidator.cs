using Microsoft.AspNetCore.Identity;
using Net_Core_Identity_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.CustomValidation
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser> // interface implemente edilince asagidaki fonk. geliyor. 
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            // password alani icerisinde username var mı yok mu kontrolu. 
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                // code ile benzersiz bi' id vermis oluyoruz. 
                errors.Add(new IdentityError() { Code = "PasswordContainsUserName", Description = "Şifre alanı kullanıcı adı içeremez." });
            }

            if (password.ToLower().Contains("1234"))
            {
                errors.Add(new IdentityError() { Code = "PasswordContains1234", Description = "Şifre alanı ardışık sayı içeremez." });
            }

            if (password.ToLower().Contains(user.Email.ToLower()))
            {
                errors.Add(new IdentityError() { Code = "PasswordContainsEmail", Description = "Şifre alanı email adresiniz içeremez." });
            }

            // hata yoksa basarili donecegiz. fonk async oldugu icin task kullandik.
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            // eger hata varsa
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray())); // butun hatalar.
            }

        }
    }
}
