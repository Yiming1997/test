using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRoommCore.Models
{
    public class ChatRoomUser
    {
       [Key]
       public string Account { get; set; }
       public string Password { get; set; }
       public string UserName { get; set; }    
       public string GUID { get; set; }
       public string ProfilePicture { get; set;}
        
    }
}
