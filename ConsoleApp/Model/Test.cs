using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Model
{
    [Table("MyTest")]
    public class Test
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [StringLength(10)]
        public string itemno { get; set; }
        [StringLength(50)]
        public string itemname { get; set; }
        
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
