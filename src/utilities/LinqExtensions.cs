using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sidesaver.utilities
{
	static class LinqExtensions
	{
		public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
		{
			if (list == null)
				return -1;

			for (int i = 0; i < list.Count; i++)
			{
				if (match(list[i]))
				{
					return i;
				}
			}
			
			return -1;
		}
	}
}
