using System;
using System.Collections.Generic;

namespace SCADA_Client.Configuration
{
    internal class ConfigItemEqualityComparer : IEqualityComparer<ConfigItem>
	{
		public bool Equals(ConfigItem x, ConfigItem y)
		{
			if (string.Compare(x.Description, y.Description) == 0)
			{
				throw new ArgumentException("Configuration item description must be unique!");
			}
			if (x.StartAddress == y.StartAddress)
			{
				throw new ArgumentException("Configuration item start address must be unique!");
			}
			if (x.StartAddress != y.StartAddress)
			{
				ConfigItem lessAddress = x.StartAddress < y.StartAddress ? x : y;
				ConfigItem greaterAddress = x.StartAddress > y.StartAddress ? x : y;
				if ((ushort)(lessAddress.StartAddress + lessAddress.NumberOfRegisters) > greaterAddress.StartAddress)
				{
					throw new ArgumentException($"Address ranges are overlapping for point types of {x.RegistryType}({x.Description}) and {y.RegistryType}({y.Description})");
				}
			}
			return false;
		}

		public int GetHashCode(ConfigItem obj)
		{
			return base.GetHashCode();
		}
	}
}