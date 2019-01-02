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
                if (!context.All.Any())
                {
                }
                var t = DateTime.Now.AddMonths(1);
                var data = context.All.Where(x => x.CreateTime == t).ToList();
                List<Test> all = new List<Test>();
                var now = DateTime.Parse(DateTime.Now.ToString("s"));
                for (DateTime start = now.AddMonths(1); start < now.AddMonths(1).AddDays(1); start = start.AddSeconds(1))
                {
                    all.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = DateTime.Now.ToString("HHmmss.fff"), CreateTime = start });
                    //context.All.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = DateTime.Now.ToString("HHmmss.fff"), CreateTime = start });
                }
                var dt = all.ToDataTable();
                dt.TableName = "MyTest";
                dt.FastToDataBase();
            }
            Console.WriteLine("Hello Database.Partition.数据库分区 https://blog.csdn.net/longzuyuan/article/details/17499859");
            Console.ReadKey();
        }
    }
}
/*
 * select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from MyTest
group by  $partition.Partition_Function_By_Time(CreateTime)
 */
