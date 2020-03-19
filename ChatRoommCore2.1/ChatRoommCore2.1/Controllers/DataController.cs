using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatRoommCore.DAL;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatRoommCore.Models;

namespace ChatRoommCore2._1.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class DataController : ControllerBase
    {
       public IActionResult GetInfo()
        {
            var info = new Dictionary<string, string>();
            info.Add("info", "successfully");
            return Ok(info);
        }   
        [HttpPost]
        public IActionResult GetUserName(ChatRoomUser user)
        {
            var userInfo = new Dictionary<string, string>();
            var db = new DataContext();
            var userNames = from u in db.ChatRoomUser
                            where user.Account == u.Account
                            select new { userName = u.UserName, userProfile = u.ProfilePicture };

            string userName = userNames.First().userName.Trim();
            string userProfile = userNames.First().userProfile.Trim();
            userInfo.Add("UserName",userName);
            userInfo.Add("UserProfile",userProfile);
            return Ok(userInfo);
        }
        [HttpPost]
        public IActionResult GetUserFriends(ChatRoomUser user)
        {
            var db = new DataContext();

            var usersFriends = from uf in db.UsersRelationship     //获取该用户好友列表
                               where user.Account == uf.UserAccount
                               //select uf.FriendAccount;
                               join cru in db.ChatRoomUser
                               on uf.FriendAccount equals cru.Account
                               select new { friendName = cru.UserName.Trim(),friendAccount = cru.Account.Trim(),friendProfile = cru.ProfilePicture.Trim()};

            return Ok(usersFriends);
        }
        public IActionResult GetGUID(ChatRoomUser user)
        {
            var db = new DataContext();
            var guid = from ug in db.ChatRoomUser
                       where ug.Account == user.Account
                       select ug.GUID;
            var userGUID = new Dictionary<string, string>();
            userGUID.Add("GUID", guid.First().Trim());

            return Ok(userGUID);
        }
        [HttpPost]
        public IActionResult GetChatRecord(DoubleUsers doubleUsers)
        {
            var db = new DataContext();
            var messageList = from cm in db.ChatMessages
                              where cm.FromUserID == doubleUsers.FirstUserID && cm.ToUserID == doubleUsers.SecondUserID
                              || cm.FromUserID == doubleUsers.SecondUserID && cm.ToUserID == doubleUsers.FirstUserID
                              orderby cm.UniCode
                              select cm.MessageContent.Trim();

            return Ok(messageList);
        }
    }
}