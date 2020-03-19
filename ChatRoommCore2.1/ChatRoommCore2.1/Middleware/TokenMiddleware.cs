using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using JWT;
using JWT.Serializers;
using Newtonsoft.Json;
using ChatRoommCore.Models;


namespace ChatRoommCore.MiddleWare
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenMiddleware(RequestDelegate requestDelegate)
        {
            this._next = requestDelegate;
        }
        public Task Invoke(HttpContext context)
        {
            var authHeader = from t in context.Request.Headers where t.Key == "auth" select t.Value.FirstOrDefault();
            if (authHeader != null)
            {
                const string secretKey = "Hello World";//加密秘钥
                string token = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        byte[] key = Encoding.UTF8.GetBytes(secretKey);
                        IJsonSerializer serializer = new JsonNetSerializer();
                        IDateTimeProvider provider = new UtcDateTimeProvider();
                        IJwtValidator validator = new JwtValidator(serializer, provider);
                        IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                        IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                        var json = decoder.DecodeToObject<AuthInfo>(token, key, verify: true);

                        if (json != null) //Token验证成功
                        {
                            if (json.ExpiryDateTime < DateTime.Now)
                            {
                                return _next.Invoke(context);
                                //return context.Response.WriteAsync("Token已失效");
                            }
                            return _next.Invoke(context);

                           
                        }
                        else //Token验证失败
                        {
                            return context.Response.WriteAsync("Token验证失败!");
                        }
                    }
                    catch (Exception ex)
                    {
                        //context.Result = new EmptyResult();
                        return context.Response.WriteAsync("Token验证发生异常!");
                    }
                }
            }
            return context.Response.WriteAsync("authHeader为空!");
        }
    }
    public static class MiddleWareExtension
    {
        public static IApplicationBuilder UseTokenMiddleWare(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenMiddleware>();
        }
    }
}
