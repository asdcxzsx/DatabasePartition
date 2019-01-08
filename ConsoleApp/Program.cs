using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
                if (!context.All.Any())
                {
                }

                var sss = DateTime.Now.ToString("s");
                //string dir = @"D:\Database1";
                //if (!Directory.Exists(dir))
                //{
                //    Directory.CreateDirectory(dir);
                //}

                //DatabaseHelper.AddPartition(DateTime.Parse("2019-01-05 05:00:00"));
               // DatabaseHelper.RemovePartition(DateTime.Parse("2019-01-05 05:00:00"));

                //List<Test> all = new List<Test>();
                //var now = DateTime.Parse("2019-01-03");
                //for (DateTime start = now; start < now.AddDays(1); start = start.AddSeconds(1))
                //{
                //    all.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = DateTime.Now.ToString("HHmmss.fff"), CreateTime = start });
                //}
                //var dt = all.ToDataTable();
                //dt.TableName = "MyTest";
                //dt.FastToDataBase();
                Console.WriteLine($"Hello Database.Partition.数据库{context.Database.Connection.Database}分区 https://blog.csdn.net/longzuyuan/article/details/17499859");
            }
            Console.ReadKey();
        }
    }
}
/*
 * select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from MyTest
group by  $partition.Partition_Function_By_Time(CreateTime)
 */
