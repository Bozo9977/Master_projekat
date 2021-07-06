using Common.GDA;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NMS
{
	public class ModelCounterDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public DMSType Type { get; set; }
		public int Counter { get; set; }
	}
}