using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using ChatRoommCore.Helper;
using ChatRoommCore.Models;
using ChatRoommCore.DAL;
using Microsoft.AspNetCore.Cors;

namespace ChatRoommCore.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UsersStatusController : ControllerBase
    {
        private static string connetion = "127.0.0.1:6379";
        [HttpPost]
        public IActionResult GetOnlineFriends(ChatRoomUser user)
        {
            RedisHelper redis = new RedisHelper(connetion);
            var db = new DataContext();
            List<string> onlineFriends = new List<string>();

            var friendsAccount = from ur in db.UsersRelationship       //获取该用户所有的好友(包括在线和不在线的)
                                 where ur.UserAccount == user.Account
                                 select ur.FriendAccount;

            foreach(var f in friendsAccount)
            {
              if(redis.GetValue(f)!=null)
                {
                    onlineFriends.Add(f);
                }
            }
            return Ok(onlineFriends);
        }
        public IActionResult GetInfo()
        {
            var info = new Dictionary<string, string>();
            info.Add("info", "successfully");
            return Ok(info);
        }
    }
}