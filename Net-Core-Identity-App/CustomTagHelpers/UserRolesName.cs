using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Net_Core_Identity_App.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentityApp.CustomTagHelpers
{
    [HtmlTargetElement("td", Attributes = "user-roles")] // tag helper'in yakalayacagi tag belirleniyor. 
    public class UserRolesName : TagHelper // inheritance
    {
        public UserManager<AppUser> UserManager { get; set; }

        public UserRolesName(UserManager<AppUser> userManager)
        {
            this.UserManager = userManager; // DI ile usermanager elde ettik. 
        }

        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; } // UserId yukaridaki attribute ile eslesti. <td user-roles="@item.Id"></td> (users.cshtml)

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output) // output tag helper'dan sonraki bos alani dolduruyor. roller teker teker basiliyor. <td user-roles="@item.Id">Buraya basiyor.</td>
        {
            AppUser user = await UserManager.FindByIdAsync(UserId);

            IList<string> roles = await UserManager.GetRolesAsync(user);

            string html = string.Empty;

            roles.ToList().ForEach(x =>
            {
                html += $"<span class='badge badge-info'>  {x}  </span>";
            });

            output.Content.SetHtmlContent(html); // output ile html'i gonderiyoruz. 
        }
    }
}