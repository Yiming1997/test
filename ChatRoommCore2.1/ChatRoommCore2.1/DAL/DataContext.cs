using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatRoommCore.Models;

namespace ChatRoommCore.DAL
{
    public class DataContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //获取opsettings.json配置信息
            var config = new ConfigurationBuilder()
                              .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json")
                              .Build();

            //获取数据库连接字符串
            string conn = config.GetConnectionString("SqlConn");
            //连接数据库
            optionsBuilder.UseSqlServer(conn);
        }
        public DbSet<ChatRoomUser> ChatRoomUser { get; set; } //实体映射
        public DbSet<UsersRelationship> UsersRelationship { get; set; }
        public DbSet<ChatMessages> ChatMessages { get; set; }
    }
}
