using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp.Model;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Context context = new Context())
            {
                var all = context.All.ToList(); 
                for (int i = -50; i < 1500; i++)
                {
                    context.All.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = "2018", CreateTime = DateTime.Now.AddDays(-i) });
                }
                context.SaveChanges();
            }
            DatabaseHelper.AddFileGroup(new List<string>()
            {
                "FG_01","FG_02","FG_03"
            });
            DatabaseHelper.CreatePartitionFunction();
            DatabaseHelper.CreatePartitionScheme();
            //DatabaseHelper.CreateTable();
            Console.WriteLine("Hello Database.Partition.数据库分区 https://blog.csdn.net/longzuyuan/article/details/17499859");
            Console.ReadKey();
        }
    }
}
