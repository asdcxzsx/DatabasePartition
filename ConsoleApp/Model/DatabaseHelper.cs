using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    public class RangeStruct
    {
        public string FileGroup { get; set; }

        public string Time { get; set; }
    }

    /// <summary>
    /// http://www.cnblogs.com/knowledgesea/p/3696912.html
    /// </summary>
    public class DatabaseHelper
    {
        public static List<RangeStruct> GetRange(DateTime s, DateTime e)
        {
            List<RangeStruct> rst = new List<RangeStruct>();
            for (DateTime start = s; start <= e; start = start.AddDays(1))
            {
                rst.Add(new RangeStruct
                {
                    FileGroup = start.ToString("yyyyMMddHHmmss"),
                    Time = "'" + start.ToString("s") + "'",
                });
            }
            return rst;
        }

        public static void AddFileGroup(List<string> groups)
        {
            using (Context context = new Context())
            {
                context.Database.CreateIfNotExists();
                //context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, @"IF EXISTS (SELECT * FROM sys.partition_functions  
                //WHERE name = 'Fun_TestUnique_Id')
                //DROP PARTITION FUNCTION Fun_TestUnique_Id;
                //GO");
                groups.ForEach(x =>
                {
                    //context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,$@"DBCC SHRINKFILE ({x}, EMPTYFILE) GO
                    //ALTER DATABASE [Test] REMOVE FILE FG_TestUnique_Id_01_data");
                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"IF NOT EXISTS (SELECT * FROM sys.filegroups  
                    WHERE name = '{x}')
                    begin
                    ALTER DATABASE [Test] ADD FILEGROUP [{x}] 
                    ALTER DATABASE [Test] ADD FILE (NAME = N'{x}', FILENAME = N'D:\Database\Test\{x}.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[{x}] 
                    end");
                    //ALTER DATABASE [Test] REMOVE FILEGROUP {x};
                    //context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"ALTER DATABASE [Test]
                    //ADD FILE
                    //(NAME = N'{x}', FILENAME = N'D:\Database\{x}.ndf', SIZE = 1MB, FILEGROWTH = 1MB)
                    //TO FILEGROUP[{x}]");
                });
            }
        }
        /// <summary> 
        /// 创建分区函数
        /// </summary>
        public static void CreatePartitionFunction(List<string> times)
        {
            //List<string> dts = new List<string>();
            //DateTime dt = DateTime.Parse("00:00:00");
            //for (int i = 0; i < 24; i++)
            //{
            //    dts.Add(dt.ToString("HH:mm:ss"));
            //}
            var val = times.MergeString();
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $"IF NOT EXISTS (SELECT * FROM sys.partition_functions WHERE name = 'Partition_Function_By_Time') CREATE PARTITION FUNCTION Partition_Function_By_Time(DATETIME) AS RANGE RIGHT FOR VALUES({val})");
            }
        }
        /// <summary>
        /// 分区方案
        /// </summary>
        public static void CreatePartitionScheme(List<string> partitionLst)
        {
            using (Context context = new Context())
            {
                var val = partitionLst.Select(x => $"[{x}]").MergeString();
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                    $"IF NOT EXISTS (SELECT * FROM sys.partition_schemes WHERE name = 'Sch_Time') begin CREATE PARTITION SCHEME Sch_Time AS PARTITION Partition_Function_By_Time TO({val}) end");
            }
        }

        /// <summary>
        /// https://www.cnblogs.com/libingql/p/4087598.html 
        /// </summary>
        public static void RebuildPk()
        {
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"
                IF EXISTS (select a.name as TabName, h.name as IndexName from sys.objects as a right join sys.indexes as h on a.object_id = h.object_id where  a.type <> 's' and h.name='PK_dbo.MyTest')
                BEGIN
                ALTER TABLE [MyTest] DROP CONSTRAINT [PK_dbo.MyTest]
                ALTER TABLE [MyTest] ADD CONSTRAINT PK_MyTest PRIMARY KEY CLUSTERED (id ASC, CreateTime DESC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
                ON Sch_Time(CreateTime)
                END");
            }
            /*
              CREATE CLUSTERED INDEX IX_CreateTime ON MyTest (CreateTime) ON Sch_Time(CreateTime)
             * CREATE CLUSTERED INDEX IX_CreateDate ON Product ( CreateDate )
                ON Scheme_DateTime ( CreateDate )

             ALTER TABLE [dbo].[MyTest] ADD  CONSTRAINT [PK_dbo.MyTest_Time] PRIMARY KEY CLUSTERED 
                (
                    [id] ASC,
	                CreateTime DESC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
                ON Sch_Time(CreateTime)
            
             */
        }

        public static void CreateTable()
        {
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MyTest')
                 CREATE TABLE MyTest
                 (
	                [CreateTime] [datetime] NOT NULL,
	                [id] [uniqueidentifier] NOT NULL,
	                [itemno] [nvarchar](10) NULL,
	                [itemname] [nvarchar](50) NULL,
                )ON Sch_Time([createtime])");
            }
        }


        public static void AddPartition(DateTime time)
        {
            var filegroup = time.ToString("yyyyMMddHHmmss");
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"
                IF NOT EXISTS (SELECT * FROM sys.filegroups WHERE name = '{filegroup}')
                BEGIN
                ALTER DATABASE [Test] ADD FILEGROUP [{filegroup}];
                ALTER DATABASE [Test] ADD FILE (NAME = N'{filegroup}', FILENAME = N'D:\Database\Test\{filegroup}.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[{filegroup}];
                ALTER PARTITION SCHEME Sch_Time NEXT USED [{filegroup}];
                ALTER PARTITION FUNCTION Partition_Function_By_Time() SPLIT RANGE('{time.ToString("s")}');
                END;
                ");
            }
        }
        /// <summary>
        /// 删除（合并）一个分区表
        /// </summary>
        public static void RemovePartition(DateTime time)
        {
            using (Context context = new Context())
            {
                string file = time.ToString("yyyyMMddHHmmss");
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"
                IF EXISTS (SELECT * FROM sys.filegroups WHERE name = '{file}')
                BEGIN
                ALTER PARTITION FUNCTION Partition_Function_By_Time() MERGE RANGE ('{time}')
                DBCC SHRINKFILE ([{file}], EMPTYFILE);
                ALTER DATABASE [Test] REMOVE FILE [{file}]
                ALTER DATABASE [Test] REMOVE FILEGROUP [{file}]
                END
                ");
            }
        }
    }


    public static class DbHelper
    {
        /// <summary>    
        /// 将集合类转换成DataTable    
        /// </summary>    
        /// <param name="list">集合</param>    
        /// <returns></returns>    
        public static DataTable ToDataTable(this IList list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                result.TableName = list[0].GetType().Name;
                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                foreach (object t in list)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(t, null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            if (result.Columns.Contains("Id"))
            {
                result.Columns["Id"].SetOrdinal(0);
            }
            return result;
        }

        /// <summary>    
        /// DataTable 转换为List 集合    
        /// </summary>    
        /// <typeparam name="T">类型</typeparam>    
        /// <param name="dt">DataTable</param>    
        /// <returns></returns>    
        public static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            //创建一个属性的列表    
            List<PropertyInfo> prlist = new List<PropertyInfo>();
            //获取TResult的类型实例  反射的入口    

            Type t = typeof(T);

            //获得TResult 的所有的Public 属性 并找出TResult属性和DataTable的列名称相同的属性(PropertyInfo) 并加入到属性列表     
            Array.ForEach<PropertyInfo>(t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1) prlist.Add(p); });

            //创建返回的集合    

            List<T> oblist = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                //创建TResult的实例    
                T ob = new T();
                //找到对应的数据  并赋值    
                prlist.ForEach(p => { if (row[p.Name] != DBNull.Value) p.SetValue(ob, row[p.Name], null); });
                //放入到返回的集合中.    
                oblist.Add(ob);
            }
            return oblist;
        }

        /// <summary>    
        /// 将泛型集合类转换成DataTable    
        /// </summary>    
        /// <typeparam name="T">集合项类型</typeparam>    
        /// <param name="list">集合</param>    
        /// <returns>数据集(表)</returns>    
        public static DataTable AnyToDataTable<T>(this IList<T> list)
        {
            return ToDataTable<T>(list, null);
        }

        /// <summary>    
        /// 将泛型集合类转换成DataTable    
        /// </summary>    
        /// <typeparam name="T">集合项类型</typeparam>    
        /// <param name="list">集合</param>    
        /// <param name="propertyName">需要返回的列的列名</param>    
        /// <returns>数据集(表)</returns>    
        public static DataTable ToDataTable<T>(IList<T> list, params string[] propertyName)
        {
            List<string> propertyNameList = new List<string>();
            if (propertyName != null)
                propertyNameList.AddRange(propertyName);
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                    else
                    {
                        if (propertyNameList.Contains(pi.Name))
                            result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        if (propertyNameList.Count == 0)
                        {
                            object obj = pi.GetValue(list[i], null);
                            tempList.Add(obj);
                        }
                        else
                        {
                            if (propertyNameList.Contains(pi.Name))
                            {
                                object obj = pi.GetValue(list[i], null);
                                tempList.Add(obj);
                            }
                        }
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            if (result.Columns.Contains("Id"))
            {
                result.Columns["Id"].SetOrdinal(0);
            }
            return result;
        }

        /// <summary>
        /// SqlBulkCopy Method 
        /// </summary>
        /// <param name="dt"></param>
        public static void FastToDataBase(this DataTable dt)
        {
            if (dt.Rows.Count != 0)
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["Context"].ConnectionString))
                {
                    bulkCopy.DestinationTableName = dt.TableName;
                    bulkCopy.BatchSize = dt.Rows.Count;
                    try
                    {
                        //sw.Start();
                        bulkCopy.WriteToServer(dt);
                        //sw.Stop();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }



    public static class Extensions
    {
        public static string MergeString(this IEnumerable<string> list, string seed)
        {
            var sum = list.Where(a => !string.IsNullOrEmpty(a)).Aggregate("", (current, a) => current + (seed + a));
            if (sum.Length > 0)
            {
                sum = sum.Substring(1);
            }
            return sum;
        }

        public static string MergeString(this IEnumerable<string> list)
        {
            return MergeString(list, ",");
        }

        public static string MergeString<T>(this IEnumerable<T> list, string seed = ",")
        {
            return MergeString(list.Select(x => x.ToString()).ToList(), seed);
        }

        public static List<T> GetDistinct<T>(this IEnumerable<T> list, Func<T, T, bool> exp)
        {
            return list.Distinct(new Compare<T>(exp)).ToList();
        }
        public static Guid ParseGuid(this string guid)
        {
            Guid id;
            Guid.TryParse(guid, out id);
            return id;
        }

    }
    /// <summary>
    /// 去重复 getdistinct
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Compare<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalsComparer;
        public Compare(Func<T, T, bool> equalsComparer)
        {
            _equalsComparer = equalsComparer;
        }
        public bool Equals(T x, T y)
        {
            return null != _equalsComparer && _equalsComparer(x, y);
        }
        public int GetHashCode(T obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
