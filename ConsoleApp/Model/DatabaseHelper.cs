using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    public class DatabaseHelper
    {
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
                    ALTER DATABASE [Test] ADD FILE (NAME = N'{x}', FILENAME = N'D:\Database\{x}.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[{x}] 
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
        public static void CreatePartitionFunction()
        {
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,$@"
                IF NOT EXISTS (SELECT * FROM sys.partition_functions WHERE name = 'Partition_Function_By_Time')
                CREATE PARTITION FUNCTION
                Partition_Function_By_Time(DATETIME) AS
                RANGE RIGHT
                FOR VALUES('2018-11-01','2018-12-20')");
            }
        }
        /// <summary>
        /// 分区方案
        /// </summary>
        public static void CreatePartitionScheme()
        {
            using (Context context = new Context())
            {
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, $@"
                IF NOT EXISTS (SELECT * FROM sys.partition_schemes WHERE name = 'Sch_Time')
                begin
                CREATE PARTITION SCHEME
                Sch_Time AS
                PARTITION Partition_Function_By_Time
                TO([FG_01],[FG_02],[FG_03])
                end");
            }
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
    }
}
