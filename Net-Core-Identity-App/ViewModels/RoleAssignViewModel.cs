using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net_Core_Identity_App.ViewModels
{
    public class RoleAssignViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Exist { get; set; } // checkbox'in isaretli olup olmadigini anlamak icin.
    }
}
