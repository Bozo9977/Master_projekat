using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	public class BreakerDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
		public bool NormalOpen { get; set; }
	}

	public class Breaker : ProtectedSwitch
	{
		public Breaker() { }

		public Breaker(Breaker b) : base(b)
		{ }

		public Breaker(BreakerDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
			NormalOpen = entity.NormalOpen;
		}

		public override IdentifiedObject Clone()
		{
			return new Breaker(this);
		}

		public override object ToDBEntity()
		{
			return new BreakerDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage, NormalOpen = NormalOpen };
		}
	}
}
