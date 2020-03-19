using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace ChatRoommCore.Helper
{
    public class RedisHelper
    {
        private ConnectionMultiplexer redis { get; set; }
        private IDatabase db { get; set; }
        public RedisHelper(string connection)
        {
            redis = ConnectionMultiplexer.Connect(connection);
            db = redis.GetDatabase();
        }
        public bool SetValue(string key, string value) //存入数据
        {
            return db.StringSet(key, value);
        }
        public string GetValue(string key) //通过键名;获取数据
        {
            return db.StringGet(key);
        }
        public bool DeleteKey(string key)
        {  
            return db.KeyDelete(key);
        }
        public void ListRightPush(string queueName,string message)
        {
            db.ListRightPush(queueName,message);
        }
        //public string ListLeftPop(string queueName)
        //{  
        //    return db.ListLeftPop(queueName);
        //}
        //public async Task<long> ListLength(string queueName)
        //{ 
        //    return await db.ListLengthAsync(queueName);
        //}
        //public async Task<string> ListLeftPop(string queueName)
        //{
        //    return await db.ListLeftPopAsync(queueName);
        //}
        public string ListLeftPop(string queueName)
        {            
           return db.ListLeftPop(queueName);
        }
        public long ListLength(string queueName)
        {
            return db.ListLength(queueName);
        }

    }
}
