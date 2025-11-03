using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Interface
{

	/// <summary>
	/// 提供權重隨機抽取功能的介面。
	/// 實作此介面的類型需回傳自身的隨機抽取權重。
	/// 權重應為非負整數，0 表示不會被抽中。
	/// </summary>
	public interface IRandomWeightItem
	{
		/// <summary>
		/// IRandomByWeight實作，取得用於隨機抽取的權重值。
		/// </summary>
		/// <returns>代表權重的非負整數值。</returns>
		int GetRandomWeight();
	}

    /// <summary>
    /// 提供針對 <see cref="IRandomWeightItem"/> 實作物件的加權隨機抽取功能。
    /// </summary>
    public static class RandomWeight<T> where T : IRandomWeightItem
	{
		/// <summary>
		/// 從指定集合中依權重隨機選出一個元素。
		/// </summary>
		/// <param name="values">實作 <see cref="IRandomWeightItem"/> 的元素集合。</param>
		/// <param name="random">可選的 <see cref="Random"/> 實例，若為 null 則自動建立。</param>
		/// <returns>隨機選中的元素。</returns>
		/// <exception cref="Exception">若所有元素的權重總和小於等於 0，或邏輯出現錯誤，將拋出例外。</exception>
		public static T GetRandom(IEnumerable<T> values, Random random = null)
		{
			int sumWeight = 0;
			foreach (var value in values)
					sumWeight += Math.Max(value.GetRandomWeight(), 0);
			if (sumWeight <= 0)
				throw new Exception("沒有可抽選的項目");

			random ??= new Random();
			int r = random.Next(sumWeight);

			int current = 0;
			foreach (var value in values)
			{
				current += Math.Max(value.GetRandomWeight(), 0);
				if (r < current)
					return value;
			}
			throw new Exception("抽取機率邏輯錯誤");
		}

		/// <summary>
		/// 嘗試從指定集合中依權重隨機選出一個元素。
		/// </summary>
		/// <param name="values">實作 <see cref="IRandomWeightItem"/> 的元素集合。</param>
		/// <param name="result">若選取成功，為隨機選中的元素；否則為 default。</param>
		/// <param name="random">可選的 <see cref="Random"/> 實例，若為 null 則自動建立。</param>
		/// <returns>若選取成功則為 <c>true</c>，否則為 <c>false</c>。</returns>
		public static bool TryGetRandom(IEnumerable<T> values, out T result, Random random = null)
		{
			result = default;

			int sumWeight = 0;
			foreach (var value in values)
				sumWeight += Math.Max(value.GetRandomWeight(), 0);
			if (sumWeight <= 0)
				return false;

			random ??= new Random();
			int r = random.Next(sumWeight);

			int current = 0;
			foreach (var value in values)
			{
				current += Math.Max(value.GetRandomWeight(), 0);
				if (r < current)
				{
					result = value;
					return true;
				}
			}
			throw new Exception("抽取機率邏輯錯誤");
			return false;
		}
	}
}
