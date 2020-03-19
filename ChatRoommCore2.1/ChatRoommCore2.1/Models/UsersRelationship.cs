using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRoommCore.Models
{
    public class UsersRelationship
    {
        [Key]
        public int UniCode { get; set; }
        public string UserAccount { get; set; }
        public string FriendAccount { get; set; }
    }
}
