using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class ICollectionExtension
	{
		/// <summary>
		/// 產生一個從startNum(包含)到endNum(包含)的整數集合
		/// </summary>
		/// <param name="startNum">集合的開始數字(包含)</param>
		/// <param name="endNum">集合的結束數字(包含)</param>
		/// <returns></returns>
		public static TCollection NumberNToMList<TCollection>(int startNum, int endNum)
			where TCollection : ICollection<int>, new()
		{
			int addDiction = startNum <= endNum ? 1 : -1;
			TCollection number = new TCollection();
			for (int i = startNum;
				addDiction == 1 ? i <= endNum : i >= endNum;
				i += addDiction)
			{
				number.Add(i);
			}
			return number;
		}
    }
}
