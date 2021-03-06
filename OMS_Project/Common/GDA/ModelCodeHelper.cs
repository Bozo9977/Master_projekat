﻿using System;
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

		public static DMSType GetTypeFromGID(long gid)
		{
			unchecked
			{
				return (DMSType)(short)((gid >> 32) & 0xFFFF);
			}
		}

		public static int GetEntityIdFromGID(long gid)
		{
			unchecked
			{
				return (int)gid;
			}
		}

		public static long CreateGID(short systemId, DMSType type, int entityId)
		{
			unchecked
			{
				return (long)(uint)entityId | ((long)(ushort)type << 32) | ((long)(ushort)systemId << 48);
			}
		}

		public static DMSType GetTypeFromModelCode(ModelCode code)
		{
			return (DMSType)(((long)code & (long)0x00000000ffff0000) >> 16);
		}

		public static PropertyType GetPropertyTypeFromModelCode(ModelCode code)
		{
			return (PropertyType)((long)code & (long)0xff);
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
				return (oldGid & ((long)(uint)~0 << 32)) | (uint)idNew;
			}
		}

		public static long SetSystemIdInGID(long oldGid, short systemId)
		{
			unchecked
			{
				return oldGid & ~((long)(ushort)~0 << 48) | ((long)(ushort)systemId << 48);
			}
		}

		public static string DMSTypeToName(DMSType type)
		{
			return Enum.GetName(typeof(DMSType), type);
		}

		public static bool ModelCodeClassIsSubClassOf(ModelCode child, ModelCode parent)
		{
			uint childUpper = GetHierearchyFromModelCode(child);
			uint parentUpper = GetHierearchyFromModelCode(parent);
			uint mask = 0xF0000000u;

			while(mask != 0)
			{
				if((mask & childUpper) != (mask & parentUpper))
					return (mask & parentUpper) == 0;

				mask >>= 4;
			}

			return true;
		}

		public static uint GetHierearchyFromModelCode(ModelCode mc)
		{
			return (uint)((ulong)mc >> 32);
		}

		public static bool IsClass(ModelCode mc)
		{
			return GetPropertyTypeFromModelCode(mc) == PropertyType.Empty;
		}

		public static bool IsProperty(ModelCode mc)
		{
			return GetPropertyTypeFromModelCode(mc) != PropertyType.Empty;
		}
	}
}
