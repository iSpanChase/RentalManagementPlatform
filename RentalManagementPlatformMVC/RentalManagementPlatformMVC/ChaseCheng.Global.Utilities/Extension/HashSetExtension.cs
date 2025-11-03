using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class HashSetExtension<T>
	{
		/// <summary>
		/// 取得隨機的唯一列表，項目來源由source取得
		/// </summary>
		/// <param name="listSize">隨機列表大小</param>
		/// <param name="source">隨機來源</param>
		/// <param name="random">若輸入，可指定Random class、種子碼</param>
		/// <returns></returns>
		public static HashSet<T> GetRandomUniqueItems(int listSize, ICollection<T> source, Random random = null)
		{
			random = random ?? new Random();

			HashSet<T> randomHashSet = new HashSet<T>();
			do
			{
				randomHashSet.Add(source.ElementAt(random.Next(0, source.Count)));
			} while (randomHashSet.Count < listSize);

			return randomHashSet;
		}

		/// <summary>
		/// 產生一個從startNum(包含)到endNum(包含)的整數HashSet集合
		/// </summary>
		/// <param name="startNum">HashSet集合的開始數字(包含)</param>
		/// <param name="endNum">HashSet集合的結束數字(包含)</param>
		/// <returns></returns>
		public static HashSet<int> NumberNToMList(int startNum, int endNum)
		{
			return ICollectionExtension.NumberNToMList<HashSet<int>>(startNum, endNum);
		}
	}
}
