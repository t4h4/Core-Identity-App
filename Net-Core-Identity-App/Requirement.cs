using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App
{
    public class ExpireDateExchangeRequirement : IAuthorizationRequirement // ExpireDateExchangeRequirement sinifi, startup tarafinda gereksinim olarak kullanilacak.
    {
    }

    public class ExpireDateExchangeHandler : AuthorizationHandler<ExpireDateExchangeRequirement> // AuthorizationHandler, kullanici member controller'da daha ilgili action'a gelmeden, authorization & authentication adimlarinda bizim degerlerimizi de kontrol etmesini sagliyor. ExpireDateExchangeHandler yazdigimiz custom handler. 
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExpireDateExchangeRequirement requirement)
        {
            if (context.User != null & context.User.Identity != null) // user ve identity null olmadigi surece
            {
                var claim = context.User.Claims.Where(x => x.Type == "ExpireDateExchange" && x.Value != null).FirstOrDefault(); // kullanici claim'i icerisinde "ExpireDateExchange" tipi aramasi ve bu tipe ait value degeri var mi kontrol ediliyor. 
                
                if (claim != null)
                {
                    if (DateTime.Now < Convert.ToDateTime(claim.Value)) // 30 gunu asmamissa

                    {
                        context.Succeed(requirement); // Succeed metodu authorization isleminin basarili oldugunda. 
                    }
                    else
                    {
                        context.Fail();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
