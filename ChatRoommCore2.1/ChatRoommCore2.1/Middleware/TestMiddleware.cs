using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace ChatRoommCore.Middleware
{
    public class TestMiddleware
    {
        private readonly RequestDelegate _next; 
       
        public TestMiddleware(RequestDelegate requestDelegate)
        {
            this._next = requestDelegate;   
        }
        public async Task Invoke(HttpContext context)
        {
            if(!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            if(context.Request.Query["type"]=="chat")
            {   
                ConcurrentDictionary<string, WebSocket> _socket = new ConcurrentDictionary<string, WebSocket>();
                CancellationToken ct = context.RequestAborted;
                WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();//获取客户端socket
                string myID = context.Request.Query["ID"];  //获取自己的ID
                string friendID = context.Request.Query["FriendID"]; //获取朋友的ID
                  
            }
        }
    }
    public static class TestMiddlewareExtension
    {
        public static IApplicationBuilder UseTestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TestMiddleware>();           
        }
    }
}
