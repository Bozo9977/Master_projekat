using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
	public class AddressMapping
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Address { get; set; }
		public long GID { get; set; }
	}
}
