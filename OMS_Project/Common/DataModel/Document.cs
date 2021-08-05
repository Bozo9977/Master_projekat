namespace Common.DataModel
{
	public abstract class Document : IdentifiedObject
	{
		public Document()
		{ }

		public Document(Document d) : base(d)
		{ }
	}
}
