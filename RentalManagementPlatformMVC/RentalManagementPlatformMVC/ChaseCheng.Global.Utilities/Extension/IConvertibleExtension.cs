using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class IConvertibleExtension
	{
		public static bool Between<T>(this T value, T start, T end)
		where T : IComparable<T>
		{
			return value.CompareTo(start) >= 0 && value.CompareTo(end) <= 0;
		}
		public static bool Between(this IConvertible value, IConvertible start, IConvertible end)
		{
			double val = Convert.ToDouble(value);
			double s = Convert.ToDouble(start);
			double e = Convert.ToDouble(end);
			return val >= s && val <= e;
		}

	}
}
