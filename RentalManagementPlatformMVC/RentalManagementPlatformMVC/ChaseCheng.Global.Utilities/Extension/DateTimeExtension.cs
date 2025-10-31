using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class DateTimeExtension
	{
		public static bool IsWorkingDay(this DateTime date)
		{
			int weekday = int.Parse(date.DayOfWeek.ToString("d"));
			return weekday >= 1 && weekday <= 5;
		}
		public static bool Between(this DateTime time, DateTime start, DateTime end)
		{
			return time > start && time < end;
		}
	}
}
