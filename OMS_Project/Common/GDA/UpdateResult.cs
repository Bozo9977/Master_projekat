using System;
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
			sb.AppendFormat("Message: {0}\n", message);
			sb.AppendLine("GID pairs:");

			foreach(KeyValuePair<long, long> kvp in globalIdPairs)
			{
				sb.AppendFormat("Client GID: 0x{0:x16}\t -> Server GID: 0x{1:x16}\n", kvp.Key, kvp.Value);
			}

			return sb.ToString();
		}
	}

	public enum ResultType : byte
	{
		Success = 0,
		Failure = 1
	}
}
