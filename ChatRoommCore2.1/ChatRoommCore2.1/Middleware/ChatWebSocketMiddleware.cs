using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.WebSockets;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ChatRoommCore.Models;
using ChatRoommCore.Helper;
using ChatRoommCore.DAL;

namespace ChatRoommCore.Middleware
{
    public class ChatWebSocketMiddleware
    {
        private static ConcurrentDictionary<string, WebSocket> _socket =
        new ConcurrentDictionary<string, WebSocket>();

        static  string connection = "127.0.0.1:6379";
        static  RedisHelper redis = new RedisHelper(connection);

        private readonly RequestDelegate _next;
         
        public ChatWebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {         
            if (!context.WebSockets.IsWebSocketRequest) //判断是否为WebSocKet请求，如果不是则执行下一个中间件
            {
                await _next.Invoke(context);
                return;
            }

                CancellationToken ct = context.RequestAborted;
                WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();//获取客户端socket                                                                         
                    
                string socketId = context.Request.Query["ID"];
               // string GUID = context.Request.Query["GUID"];
    
                redis.SetValue(socketId, "online"); //在redis保存登陆状态
                _socket.TryAdd(socketId, currentSocket);
               //await SendSocketsNum(ct);  //每当新登陆进来一个用户时，发送一次在线人数
                Task t = SendUserOnlineFriensd(ct, socketId);//向用户主动推送他的在线好友

            //var receivedMessOffLine="";

            //do
            //{
            //    receivedMessOffLine = redis.ListLeftPop(GUID);

            //    if (receivedMessOffLine != null)
            //    {
            //        await SendStringAsync(currentSocket, receivedMessOffLine, ct);
            //    }

            //} while (receivedMessOffLine != null);

            while (true)
                {        
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
              
                var response = await ReceiveSrtingAsync(currentSocket, socketId, ct);
                    if (string.IsNullOrEmpty(response)) //下线
                    {
                        if (currentSocket.State != WebSocketState.Open)
                        {
                            break;
                        }
                        continue;
                    }

                string targetFriendID = response.Split("$Friend$")[0];
                WebSocket friendSocket;
                _socket.TryGetValue(targetFriendID, out friendSocket); //获取好友的socket
                WebSocket mySocket;
                _socket.TryGetValue(socketId, out mySocket); //获取自己的socket
                string message = "";
                var db = new DataContext();
               
                for (int i = 0;i< response.Split("$Friend$").Length;i++)
                {
                    if(i==0)
                    {
                        continue;
                    }
                    message += response.Split("$Friend$")[i];
                }
                if (friendSocket == null) //如果好友不在线
                {
                  //string friendGUID = GetFriendGUID(targetFriendID);
                  //redis.ListRightPush(friendGUID, message);
                    await SendStringAsync(mySocket, message, ct);                 
                }
                else                      //若好友在线
                {
                    await SendStringAsync(mySocket, message, ct); //发给自己
                    await SendStringAsync(friendSocket, message, ct);//发给好友
                }
                db.ChatMessages.Add(new ChatMessages() { FromUserID = socketId, MessageContent = message, ToUserID = targetFriendID }); //把该消息存到数据库中
                db.SaveChanges();

                //foreach (var socket in _socket) //broadcast 广播
                //{
                //    if (socket.Value.State != WebSocketState.Open)
                //    {
                //        continue;
                //    }
                //    await SendStringAsync(socket.Value, response, ct);
                //}
            }
        }
        private static async Task SendStringAsync(WebSocket socket,string data,CancellationToken ct = default(CancellationToken))
        {
            List<string> list = new List<string>();
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
        }
        private static async Task<string> ReceiveSrtingAsync(WebSocket socket,string socketId,CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream()) //中间对象，用于存取字节流
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();
                    result = await socket.ReceiveAsync(buffer, ct); //接收信息，并存入buffeer缓存器（以二进制流的形式）
                    ms.Write(buffer.Array, buffer.Offset, result.Count); //写入字节流
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);//Seek方法，将流中的位置设置为指定值
                if(result.MessageType!=WebSocketMessageType.Text)
                {    
                    if(result.MessageType==WebSocketMessageType.Close)
                    {
                        redis.DeleteKey(socketId);//如果有用户离线，那么就在redis删掉他的在线状态
                        _socket.TryRemove(socketId, out socket); //如果有用户退出聊天室，那么就删除其在服务端的socket
                      // await SendSocketsNum(ct);（目前暂时不用）每当有一个用户退出时，发送一次在线人数
                    }
                    return null;
                }

                using (var reader = new StreamReader(ms,Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
        //private static async Task SendSocketsNum(CancellationToken ct)
        //{
        //     string socketsNum ="#server#allUsers:"+_socket.Count;
        //    foreach(var socket in _socket)
        //    {
        //        if(socket.Value.State != WebSocketState.Open)
        //        {
        //            continue;
        //        }
        //        await SendStringAsync(socket.Value, socketsNum, ct);
        //    }
        //}
        private static async Task SendUserOnlineFriensd(CancellationToken ct,string socketId)
        {
            var db = new DataContext();

            await Task.Run(async () =>
            {
                while(true)
                {
                    Thread.Sleep(3000);
                    string onlineFriends = "#server#userFriends:";

                    var friendsAccount = from ur in db.UsersRelationship       //获取该用户所有的好友(包括在线和不在线的)
                                         where ur.UserAccount == socketId
                                         select ur.FriendAccount.Trim();

                    foreach (var f in friendsAccount)                         //获取该用户所有在线的好友
                    {
                        if (redis.GetValue(f.Trim()) != null)
                        {
                            onlineFriends += f + " ";
                        }
                    }
                    WebSocket mySocket;
                    _socket.TryGetValue(socketId, out mySocket);

                    await SendStringAsync(mySocket, onlineFriends, ct);
                }
            });       
        }
        private static string GetFriendGUID(string socketID)
        {
            var db = new DataContext();

            var friendGUID = from ug in db.ChatRoomUser
                             where ug.Account == socketID
                             select ug.GUID.Trim();

            return friendGUID.First();
        }
    }
}
