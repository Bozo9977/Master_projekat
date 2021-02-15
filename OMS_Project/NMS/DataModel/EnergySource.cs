using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	public class EnergySourceDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
	}

	public class EnergySource : ConductingEquipment
	{
		public EnergySource() { }

		public EnergySource(EnergySource e) : base(e)
		{ }

		public EnergySource(EnergySourceDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
		}

		public override IdentifiedObject Clone()
		{
			return new EnergySource(this);
		}

		public override object ToDBEntity()
		{
			return new EnergySourceDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage };
		}
	}
}
