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
            Random random = new Random();
            using (Context context = new Context())
            {
                var st = DateTime.Now.AddDays(-20);
                var end = DateTime.Now.AddDays(20);
                var alist = context.All.Where(x => x.CreateTime > st && x.CreateTime < end).ToList();
                if (!context.All.Any())
                {
                    List<Test> all = new List<Test>();
                    for (DateTime start = DateTime.Now.AddDays(-30); start < DateTime.Now.AddMonths(1); start = start.AddMinutes(1))
                    {
                        all.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = DateTime.Now.ToString("HHmmss.fff"), CreateTime = start });
                        //context.All.Add(new Test() { itemname = Guid.NewGuid().ToString(), itemno = DateTime.Now.ToString("HHmmss.fff"), CreateTime = start });
                    }
                    var dt = all.ToDataTable();
                    dt.TableName = "MyTest";
                    dt.FastToDataBase();
                }
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
