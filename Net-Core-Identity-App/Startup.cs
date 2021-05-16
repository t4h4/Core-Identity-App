using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net_Core_Identity_App.CustomValidation;
using Net_Core_Identity_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // entity servis ekleme
            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(Configuration["ConnectionStrings:DefaultConnectionString"]);
                // opts.UseSqlServer(configuration["ConnectionStrings:DefaultAzureConnectionString"]);
            });


            // identity servis ekleme
            services.AddIdentity<AppUser, AppRole>(opts => {
                // sifre dogrulama ayarlari 
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;

                // kullanici dogrulama ayarlari 
                opts.User.RequireUniqueEmail = true; // mail benzersiz olmali. 
                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.useroptions.allowedusernamecharacters?view=aspnetcore-2.2
                opts.User.AllowedUserNameCharacters = "abcçdefgðhýijklmnoöçpqrsþtuüvwxyzABCÇDEFGÐHIÝJKLMNOÖPQRSTUÜVWXYZ0123456789-._";


            })
                .AddPasswordValidator<CustomPasswordValidator>() // ozel sifre validasyon sinifi eklendi.
                .AddUserValidator<CustomUserValidator>() // ozel kullanici validasyon sinifi eklendi. 
                .AddErrorDescriber<CustomIdentityErrorDescriber>() // dogrulama mesajlarini turkcelestiren sinif eklendi. 
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders(); // token'lar kullanmak icin gerekli servisi ekledik.

            // kaydedilecegi yer <AppIdentityDbContext> bunu saglayan func. AddEntityFrameworkStores
            // <AppIdentityDbContext> , <AppUser, IdentityRole> entity'deki varliklari sql server'da tablolari olusturacak.



            // Cookie bazli kimlik dogrulama servisi
            CookieBuilder cookieBuilder = new CookieBuilder();

            cookieBuilder.Name = "MyBlog";
            cookieBuilder.HttpOnly = false; // client side cookie bilgisi okunmayacak. sadece http istegi uzerinden cookie bilgisini almak istiyorum.
            cookieBuilder.SameSite = SameSiteMode.Lax; // siteler arasi cookie paylasimi acik. alt domain'de ayný cookie yapisini kullanabilir. strict olsaydi kisitli olurdu. detaylar icin anahtar Cross-Site Request Forgery (CSRF)
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest; // always olsaydý sadece https istegiyle cookie sunucuya giderdi. SameAsRequest hangisinden gelirse ondan gonderiyor http ya da https farketmez. ilk http'den kaydedildiyse sadece http'den gonderir. ama none olursa bu da onemsiz olur. ne olursa olsun gonderir. 

            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = new PathString("/Home/Login"); // yetki olmayan sayfalara girince buraya gidecek. 
                opts.LogoutPath = new PathString("/Member/LogOut"); // path'in yaptigi islem asp-route-returnUrl="/Home/Index" 'e yonlendirme yapmasi. 
                opts.Cookie = cookieBuilder;
                opts.SlidingExpiration = true; // cookie omrunun yarisi gectikten sonra kullanici tekrar istek yaparsa default olarak sure tekrar 60 gune cikarilir. 
                opts.ExpireTimeSpan = System.TimeSpan.FromDays(60); // 60 gun boyunca cookie bilgisini tutacak. 
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied"); // uye kullanici yetkisiz oldugu sayfaya girmeye calistiginda buradaki path'e yonlendirilip, bilgilendirilecek.
            });



            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
