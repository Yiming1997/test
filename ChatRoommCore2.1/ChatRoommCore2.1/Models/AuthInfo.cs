using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRoommCore.Models
{
    public class AuthInfo
    {
        public string Account { get; set; }
        public List<string> Roles { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? ExpiryDateTime { get; set; }
    }
}