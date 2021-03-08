using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public class RecloserDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
		public bool NormalOpen { get; set; }
	}

	public class Recloser : ProtectedSwitch
	{
		public Recloser() { }

		public Recloser(Recloser r) : base(r)
		{ }

		public Recloser(RecloserDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
			NormalOpen = entity.NormalOpen;
		}

		public override IdentifiedObject Clone()
		{
			return new Recloser(this);
		}

		public override object ToDBEntity()
		{
			return new RecloserDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage, NormalOpen = NormalOpen };
		}
	}
}
