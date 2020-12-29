using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	public static class ModelCodeHelper
	{
		public static short GetSystemIdFromGID(long gid)
		{
			unchecked
			{
				return (short)((gid >> 48) & 0xFFFF);
			}
		}

		public static short GetTypeFromGID(long gid)
		{
			unchecked
			{
				return (short)((gid >> 32) & 0xFFFF);
			}
		}

		public static int GetEntityIdFromGID(long gid)
		{
			unchecked
			{
				return (int)gid;
			}
		}

		public static long CreateGID(short systemId, short type, int entityId)
		{
			unchecked
			{
				return (long)(uint)entityId | ((long)(ushort)type << 32) | ((long)(ushort)systemId << 48);
			}
		}

		public static DMSType GetTypeFromModelCode(ModelCode code)
		{
			return (DMSType)(((long)code & (long)ModelCodeMask.MASK_TYPE) >> 16);
		}

		public static bool GetModelCodeFromString(string strModelCode, out ModelCode modelCode)
		{
			return Enum.TryParse(strModelCode, true, out modelCode);
		}

		public static bool GetTypeFromString(string strType, out DMSType type)
		{
			return Enum.TryParse(strType, true, out type);
		}

		public static long SetEntityIdInGID(long oldGid, int idNew)
		{
			unchecked
			{
				return (oldGid & ~((long)(uint)~0 << 32)) | (uint)idNew;
			}
		}

		public static long SetSystemIdInGID(long oldGid, short systemId)
		{
			unchecked
			{
				return oldGid & ~((long)(ushort)~0 << 48) | ((long)(ushort)systemId << 48);
			}
		}
	}
}
