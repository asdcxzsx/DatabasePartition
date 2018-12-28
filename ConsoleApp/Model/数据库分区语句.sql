USE [Test] --https://docs.microsoft.com/zh-cn/sql/relational-databases/partitions/modify-a-partition-function?view=sql-server-2017
GO
 --1.创建文件组
ALTER DATABASE [Test]
ADD FILEGROUP [FG_TestUnique_Id_01]

ALTER DATABASE [Test]
ADD FILEGROUP [FG_TestUnique_Id_02]

ALTER DATABASE [Test]
ADD FILEGROUP [FG_TestUnique_Id_03]
--2.创建文件
ALTER DATABASE [Test]
ADD FILE
(NAME = N'FG_TestUnique_Id_01_data',FILENAME = N'D:\Database\FG_TestUnique_Id_01_data.ndf',SIZE = 1MB, FILEGROWTH = 1MB )
TO FILEGROUP [FG_TestUnique_Id_01];

ALTER DATABASE [Test]
ADD FILE
(NAME = N'FG_TestUnique_Id_02_data',FILENAME = N'D:\Database\FG_TestUnique_Id_02_data.ndf',SIZE = 1MB, FILEGROWTH = 1MB )
TO FILEGROUP [FG_TestUnique_Id_02];

ALTER DATABASE [Test]
ADD FILE
(NAME = N'FG_TestUnique_Id_03_data',FILENAME = N'D:\Database\FG_TestUnique_Id_03_data.ndf',SIZE = 1MB, FILEGROWTH = 1MB )
TO FILEGROUP [FG_TestUnique_Id_03];

--3.创建分区函数
--我们创建了一个用于数据类型为int的分区函数，按照数值来划分
--文件组                  分区      取值范围
--[FG_TestUnique_Id_01]    1        (小于2, 2]--包括2
--[FG_TestUnique_Id_02]    2        [3, 4]
--[FG_TestUnique_Id_03]    3        (4,大于4)  --不包括4
--
--ALTER PARTITION SCHEME [Sch_TestUnique_Id]
 
--DROP PARTITION SCHEME Sch_Time
--DROP TABLE testPartionTable
IF EXISTS (SELECT * FROM sys.partition_functions  
    WHERE name = 'Fun_TestUnique_Id')  
    DROP PARTITION FUNCTION Fun_TestUnique_Id;  
GO  
DBCC SHRINKFILE (FG_02, EMPTYFILE);
ALTER DATABASE [Test] REMOVE FILE FG_02
ALTER DATABASE [Test] REMOVE FILEGROUP FG_02
SELECT * FROM sys.schemas  

CREATE PARTITION FUNCTION
Fun_TestUnique_Id(DATETIME) AS
RANGE RIGHT
FOR VALUES('2018-12-26','2018-12-27')
--4.创建分区方案
CREATE PARTITION SCHEME
Sch_TestUnique_Id AS
PARTITION Fun_TestUnique_Id
TO([FG_TestUnique_Id_01],[FG_TestUnique_Id_02],[FG_TestUnique_Id_03])
--5.创建分区表
CREATE TABLE testPartionTable
(
  id INT  NOT NULL,
  itemno CHAR(20),
  itemname CHAR(40),
  createtime DATETIME NOT NULL
)ON Sch_TestUnique_Id([createtime])

INSERT INTO [dbo].[testPartionTable] ( [id], [itemno], [itemname],[createtime] )
SELECT 1,'1','中国','2018-12-26' UNION ALL
SELECT 2,'2','法国','2018-12-26' UNION ALL
SELECT 3,'3','美国','2018-12-27' UNION ALL
SELECT 4,'4','英国','2018-12-27' UNION ALL
SELECT 5,'5','德国','2018-12-28'


--truncate table testPartionTable
--delete from testPartionTable where id>0 https://www.cnblogs.com/libingql/p/4087598.html

IF not EXISTS (SELECT * FROM sys.partition_functions  
    WHERE name = 'Fun_TestUnique_Id')  
begin 
SELECT * FROM sys.partition_functions  
SELECT * FROM sys.tables  
end

exec sp_helpindex N'MyTest'

IF NOT EXISTS (SELECT * FROM sys.partitions WHERE name = 'Sch_Time')
                begin
                CREATE PARTITION SCHEME
                Sch_Time AS
                PARTITION Partition_Function_By_Time
                TO([FG_01],[FG_02],[FG_03])
                end

				drop table MyTest

				 CREATE TABLE MyTest
                 (
	                [CreateTime] [datetime] NOT NULL,
	                [id] [uniqueidentifier] NOT NULL,
	                [itemno] [nvarchar](10) NULL,
	                [itemname] [nvarchar](50) NULL,
                )ON Sch_Time([createtime])

INSERT INTO [dbo].MyTest ( [id], [itemno], [itemname],[CreateTime] )
SELECT 'cf9630fd-2332-4aaa-8948-000326bdb4ac','223561','中国22','2018-11-26 18:00:05' 


select * from sys.partition_range_values

drop table MyTest

--ALTER TABLE MyTable
--ADD
--PRIMARY KEY NONCLUSTERED(_ID,KeepFlag)
--ON Data_Scheme(KeepFlag)
--GO
ALTER TABLE MyTest
ADD
PRIMARY KEY CLUSTERED(id,CreateTime)
ON Sch_Time(CreateTime)

DROP PARTITION FUNCTION Partition_Function_By_Time; 
DROP PARTITION SCHEME Sch_Time;

drop table MyTest
----------------------------------------------------------------------------------------------------------------------
select [CreateTime] from MyTest where [CreateTime]>'2018-10-10'

SELECT  * FROM  sys.dm_db_index_physical_stats(DB_ID('test'), OBJECT_ID('MyTest'), NULL,NULL, 'detailed')

alter table [MyTest]
DROP CONSTRAINT [PK_dbo.MyTest]

ALTER TABLE [dbo].[MyTest] ADD  CONSTRAINT [PK_dbo.MyTest] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	CreateTime desc
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
ON Sch_Time(CreateTime)
GO


select * from MyTest where createtime>'2016-10-15'