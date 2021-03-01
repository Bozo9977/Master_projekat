using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SCADA_Common.DB_Model
{
    public class PointItemDB
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [Index(IsUnique= true)]
        public long Gid { get; set; }
        public ushort Address { get; set; }
        public string Name { get; set; }
        public PointType RegisterType { get; set; }
        public bool Alarm { get; set; }

    }
}
