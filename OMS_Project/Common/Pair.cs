namespace Common
{
	public struct Pair<T1, T2>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }

		public Pair(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}

	public struct Triple<T1, T2, T3>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }
		public T3 Third { get; private set; }

		public Triple(T1 first, T2 second, T3 third)
		{
			First = first;
			Second = second;
			Third = third;
		}
	}
}
