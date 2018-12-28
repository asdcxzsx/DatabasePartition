using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    public class Context : DbContext
    {
        public Context()
            : base("Context")
        {
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
            Database.SetInitializer(new DbSetup());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Test>().ToTable("MyTest", "Sch_TestUnique_Id");
            //modelBuilder.Entity<CommandLog>().HasIndex(b => b.appId);
            // modelBuilder.Entity<AlarmLog>().HasIndex(c => c.Entity.Id);
        }

        public DbSet<Test> All { get; set; }
    }
    public class DbSetup : MigrateDatabaseToLatestVersion<Context, DbMigrate>
    {

    }

    public class DbMigrate : DbMigrationsConfiguration<Context>
    {
        public DbMigrate()
        {
            this.AutomaticMigrationDataLossAllowed = true;
            this.AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Context context)
        {
            InitDatabaseIndex(context);
        }

        private void InitDatabaseIndex(Context context)
        {
            var sqlQuery = context.Database.SqlQuery<DatabaseIndexModel>(@"select a.name as TabName, h.name as IndexName
                from sys.objects as a right join sys.indexes as h on a.object_id = h.object_id where  a.type <> 's'",
                new object[] { });
            var model = sqlQuery.ToList();
            CreateIndex(context, "CreateTime", "MyTest", model, false, "[id],[itemno],[itemname]");
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="context">数据库对象</param>
        /// <param name="field">索引字段</param>
        /// <param name="table">创建的索引表</param>
        /// <param name="model">存在的索引集合</param>
        /// <param name="unique">是否是UNIQUE索引类型</param>
        private void CreateIndex(Context context, string field, string table, List<DatabaseIndexModel> model, bool unique = false, string include = "")
        {
            if (!model.Any(c => c.TabName == table && c.IndexName == $"IX_{table}_{field.Replace(",", "_")}"))
            {
                context.Database.ExecuteSqlCommand(String.Format(
                    "CREATE {0}NONCLUSTERED INDEX IX_{1}_{2} ON {5} ({3}) {4}", //可以 include(id, Description) 去除sql key look up
                    unique ? "UNIQUE " : "",
                    table,
                    field.Replace(",", "_"),
                    field,
                    string.IsNullOrEmpty(include) ? "" : $"INCLUDE({include})",
                    $"[{table}]"
                ));
            }
        }

        public class DatabaseIndexModel
        {
            public string TabName { get; set; }

            public string IndexName { get; set; }
        }
    }
}
