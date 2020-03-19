using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using ChatRoommCore.DAL;
using ChatRoommCore.Models;

namespace ChatRoommCore.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login(ChatRoomUser user)
        {
            Dictionary<string, bool> dMsg = new Dictionary<string, bool>();
            var db = new DataContext();

            bool exits = db.ChatRoomUser.Any(u => u.Account.Trim() == user.Account.Trim() && u.Password.Trim() == user.Password.Trim());//账号密码验证

            if (exits)                        //登陆成功
            {
                dMsg.Add("Success", true);
                return Ok(dMsg);
            }
            else                             //登陆失败
            {
                dMsg.Add("Success", false);
                return Ok(dMsg);
            }
        }
    }
}