using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class ListExtension
	{
		/// <summary>
		/// 產生一個從startNum(包含)到endNum(包含)的整數list列表
		/// </summary>
		/// <param name="startNum">list的開始數字(包含)</param>
		/// <param name="endNum">list的結束數字(包含)</param>
		/// <returns></returns>
		public static List<int> NumberNToMList(int startNum, int endNum)
		{
			return ICollectionExtension.NumberNToMList<List<int>>(startNum, endNum);
		}
	}
}
