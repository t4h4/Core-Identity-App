using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Net_Core_Identity_App.Models
{
    // dbo.AspNetUsers database tablosuna karsilik geliyor. 
    // identityUser default gelen propertyleri veritabanina zaten aktardi. 
    public class AppUser : IdentityUser
    {
        public string City { get; set; }
        public string Picture { get; set; }
        public DateTime? BirthDay { get; set; } // ? ile null olabilecek datetime olmasi lazim. kullanici uye olurken bunu belirtmiyor. 
        public int Gender { get; set; }
    }
}
