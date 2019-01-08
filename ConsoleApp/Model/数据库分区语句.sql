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


select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from MyTest
group by  $partition.Partition_Function_By_Time(CreateTime)



/*********************************************************/
IF NOT EXISTS (SELECT * FROM sys.partition_functions 
WHERE name = 'Partition_Function_By_Time_0') CREATE PARTITION FUNCTION Partition_Function_By_Time_0(DATETIME) AS RANGE LEFT FOR VALUES('2019-02-01')

IF NOT EXISTS (SELECT * FROM sys.partition_schemes WHERE name = 'Sch_Time_01')
 begin CREATE PARTITION SCHEME Sch_Time_01 AS PARTITION Partition_Function_By_Time_0 TO([Test01],[Test02]) end

 alter table [MyTest] DROP CONSTRAINT [PK_dbo.MyTest_Time]
  ALTER TABLE [dbo].[MyTest] ADD  CONSTRAINT [PK_dbo.MyTest_Time] PRIMARY KEY CLUSTERED 
                (
                    [id] ASC,
	                CreateTime DESC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
                ON Sch_Time(CreateTime)


				select * from MyTest where CreateTime='2019-02-02 16:28:00' order by CreateTime asc

				--delete from MyTest where CreateTime>'2019-02-01'
				 ALTER DATABASE [Test] ADD FILEGROUP [Test01] 
                    ALTER DATABASE [Test] ADD FILE (NAME = N'Test01', FILENAME = N'D:\Database\Test01.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[Test01] 
					ALTER DATABASE [Test] ADD FILEGROUP [Test02] 
                    ALTER DATABASE [Test] ADD FILE (NAME = N'Test02', FILENAME = N'D:\Database\Test02.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[Test02] 



--https://www.cnblogs.com/fyen/archive/2011/01/18/1938707.html
ALTER PARTITION SCHEME Sch_Time NEXT USED [20190104]
ALTER PARTITION FUNCTION Partition_Function_By_Time() SPLIT RANGE('2019-01-04')

SELECT * FROM sys.schemas  
ALTER PARTITION FUNCTION Partition_Function_By_Time() MERGE RANGE ('2019-01-04')

DBCC SHRINKFILE ([20190104], EMPTYFILE);
ALTER DATABASE [Test] REMOVE FILE [20190104]
ALTER DATABASE [Test] REMOVE FILEGROUP [20190104]


SELECT * FROM sys.filegroups 

select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from MyTest
group by  $partition.Partition_Function_By_Time(CreateTime)

select * from sysfiles

--https://www.cnblogs.com/zzs-pedestrian/p/6525390.html
select convert(varchar(50), ps.name) as partition_scheme,
p.partition_number, 
convert(varchar(10), ds2.name) as filegroup, 
convert(varchar(19), isnull(v.value, ''), 120) as range_boundary, 
str(p.rows, 9) as rows
from sys.indexes i 
join sys.partition_schemes ps on i.data_space_id = ps.data_space_id 
join sys.destination_data_spaces dds
on ps.data_space_id = dds.partition_scheme_id 
join sys.data_spaces ds2 on dds.data_space_id = ds2.data_space_id 
join sys.partitions p on dds.destination_id = p.partition_number
and p.object_id = i.object_id and p.index_id = i.index_id 
join sys.partition_functions pf on ps.function_id = pf.function_id 
LEFT JOIN sys.Partition_Range_values v on pf.function_id = v.function_id
and v.boundary_id = p.partition_number - pf.boundary_value_on_right 
WHERE i.object_id = object_id('Login_Log')    --此处是表名
and i.index_id in (0, 1) 
order by p.partition_number












-- IF EXISTS (SELECT * FROM sys.filegroups WHERE name = '20190105000000')
--                BEGIN
--                ALTER PARTITION FUNCTION Partition_Function_By_Time() MERGE RANGE ('2019-01-05 00:00:00')
--                DBCC SHRINKFILE ([20190105000000], EMPTYFILE);
--                ALTER DATABASE [LiveMonitor] REMOVE FILE [20190105000000]
--                ALTER DATABASE [LiveMonitor] REMOVE FILEGROUP [20190105000000]
--                END

--select * from sys.sysfiles

select * from SensorLog where CreateTime between '2019-01-04 01:50:01' and '2019-01-04 23:59:59' and Sensor_Id='5a261242-335b-435b-8dae-0667f20f24d1'

DBCC SHRINKFILE (LiveMonitor)


select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from SensorLog
group by  $partition.Partition_Function_By_Time(CreateTime)

SELECT * FROM sys.filegroups 

select * from sysfiles

SELECT * FROM sys.partition_functions 

--ALTER PARTITION FUNCTION Partition_Function_By_Time() SPLIT RANGE('2019-01-05')


 IF NOT EXISTS (SELECT * FROM sys.filegroups WHERE name = '20190107000000')
                BEGIN
                ALTER DATABASE [LiveMonitor] ADD FILEGROUP [20190107000000]
                ALTER DATABASE [LiveMonitor] ADD FILE (NAME = N'20190107000000', FILENAME = N'D:\Database\20190107000000.ndf', SIZE = 1MB, FILEGROWTH = 1MB) TO FILEGROUP[20190107000000] 
                ALTER PARTITION SCHEME Sch_Time NEXT USED [20190107000000]
                ALTER PARTITION FUNCTION Partition_Function_By_Time() SPLIT RANGE('2019-01-07')
                END



select $partition.Partition_Function_By_Time(CreateTime) as partitionNum,count(*) as recordCount
from SensorLog
group by  $partition.Partition_Function_By_Time(CreateTime)


select * from sys.partition_range_values


select convert(varchar(50), ps.name) as partition_scheme,
p.partition_number, 
convert(varchar(20), ds2.name) as filegroup, 
convert(varchar(19), isnull(v.value, ''), 120) as range_boundary, 
str(p.rows, 9) as rows
from sys.indexes i 
join sys.partition_schemes ps on i.data_space_id = ps.data_space_id 
join sys.destination_data_spaces dds
on ps.data_space_id = dds.partition_scheme_id 
join sys.data_spaces ds2 on dds.data_space_id = ds2.data_space_id 
join sys.partitions p on dds.destination_id = p.partition_number
and p.object_id = i.object_id and p.index_id = i.index_id 
join sys.partition_functions pf on ps.function_id = pf.function_id 
LEFT JOIN sys.Partition_Range_values v on pf.function_id = v.function_id
and v.boundary_id = p.partition_number - pf.boundary_value_on_right 
WHERE i.object_id = object_id('SensorLog')    --此处是表名
and i.index_id in (0, 1) 
order by p.partition_number


alter table SensorLog switch partition 1 to SensorLogShadow  partition 1

truncate table SensorLogShadow

select count(*) from ExtendLog


select convert(varchar(50), ps.name) as partition_scheme,
p.partition_number, 
convert(varchar(20), ds2.name) as filegroup, 
convert(varchar(19), isnull(v.value, ''), 120) as range_boundary, 
str(p.rows, 9) as rows
from sys.indexes i 
join sys.partition_schemes ps on i.data_space_id = ps.data_space_id 
join sys.destination_data_spaces dds
on ps.data_space_id = dds.partition_scheme_id 
join sys.data_spaces ds2 on dds.data_space_id = ds2.data_space_id 
join sys.partitions p on dds.destination_id = p.partition_number
and p.object_id = i.object_id and p.index_id = i.index_id 
join sys.partition_functions pf on ps.function_id = pf.function_id 
LEFT JOIN sys.Partition_Range_values v on pf.function_id = v.function_id
and v.boundary_id = p.partition_number - pf.boundary_value_on_right 
WHERE i.object_id = object_id('ExtendLog')    --此处是表名
and i.index_id in (0, 1) 
order by p.partition_number



select count(*) from SensorLog