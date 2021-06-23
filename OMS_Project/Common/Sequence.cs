using System.Collections.Generic;

namespace Common
{
	public class Sequence<T>
	{
		IReadOnlyList<IReadOnlyList<T>> lists;
		int i;
		int j;

		public Sequence(IReadOnlyList<IReadOnlyList<T>> lists)
		{
			this.lists = lists;
		}

		public bool Reset()
		{
			i = 0;
			j = 0;
			return true;
		}

		public bool Next(out T item)
		{
			while(i < lists.Count)
			{
				IReadOnlyList<T> list = lists[i];

				if(j < list.Count)
				{
					item = list[j++];
					return true;
				}

				j = 0;
				++i;
			}

			item = default;
			return false;
		}
	}
}
