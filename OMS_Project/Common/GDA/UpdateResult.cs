﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	[DataContract]
	public class UpdateResult
	{
		[DataMember]
		Dictionary<long, long> globalIdPairs;
		[DataMember]
		string message;
		[DataMember]
		ResultType result;

		public UpdateResult(Dictionary<long, long> ids, string msg, ResultType res)
		{
			globalIdPairs = ids;
			message = msg;
			result = res;
		}

		public IReadOnlyDictionary<long, long> GlobalIdPairs
		{
			get { return globalIdPairs; }
		}
		
		public string Message
		{
			get { return message; }
		}

		public ResultType Result
		{
			get { return result; }
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("Update result: {0}\n", result);
			sb.AppendFormat("Message: {0}\n", message == null ? "N/A" : message);
			sb.Append("Count: ");
			sb.Append(globalIdPairs == null ? "N/A" : globalIdPairs.Count.ToString());
			return sb.ToString();
		}
	}

	public enum ResultType : byte
	{
		Success = 0,
		Failure = 1
	}
}
