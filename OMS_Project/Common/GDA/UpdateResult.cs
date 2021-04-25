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
		Dictionary<long, long> inserted;
		[DataMember]
		List<long> updated;
		[DataMember]
		List<long> deleted;
		[DataMember]
		string message;
		[DataMember]
		ResultType result;

		public UpdateResult(ResultType res, string msg = null, Dictionary<long, long> inserted = null, List<long> updated = null, List<long> deleted = null)
		{
			this.inserted = inserted;
			this.updated = updated;
			this.deleted = deleted;
			message = msg;
			result = res;
		}

		public IReadOnlyDictionary<long, long> Inserted
		{
			get { return inserted; }
		}

		public IReadOnlyList<long> Updated
		{
			get { return updated; }
		}

		public IReadOnlyList<long> Deleted
		{
			get { return deleted; }
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

			sb.Append("Update result: ");
			sb.AppendLine(result.ToString());
			sb.Append("Message: ");
			sb.AppendLine(message == null ? "N/A" : message);
			sb.Append("Inserted: ");
			sb.AppendLine(inserted == null ? "N/A" : inserted.Count.ToString());
			sb.Append("Updated: ");
			sb.AppendLine(updated == null ? "N/A" : updated.Count.ToString());
			sb.Append("Deleted: ");
			sb.AppendLine(deleted == null ? "N/A" : deleted.Count.ToString());

			return sb.ToString();
		}
	}

	public enum ResultType : byte
	{
		Success = 0,
		Failure = 1
	}
}
