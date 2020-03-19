using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRoommCore.Models
{
    public class ChatMessages
    {
        [Key]
        public int UniCode { get; set; }
        public string MessageContent { get; set; }
        public string FromUserID { get; set; }
        public string ToUserID { get; set; }
    }
}
