using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class IEnumerableExtension
	{
		/// <summary>
		/// 從一個集合中隨機抽取 N 個的項目，取後不放回。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="count"></param>
		/// <param name="random"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static IEnumerable<T> GetRandomUnique<T>(this IEnumerable<T> source, int count,Random random = null)
		{
			random ??= new Random();
			const float switchThreshold = 0.25f;
			if (count >= source.Count() * switchThreshold)
			{
				return GetRandomUniqueByFisherYatesToArray(source, count);
			}
			else
			{
				return GetRandomUniqueByReservoirSampling(source, count);
			}
		}

		static IEnumerable<T> GetRandomUniqueByFisherYatesToArray<T>(IEnumerable<T> source, int count, Random random = null)
		{
			if (source is null)
				throw new ArgumentNullException($"{nameof(source)}是Null");
			int sourceCount = source.Count();
			if (count < 0 || count > sourceCount)
				throw new ArgumentOutOfRangeException(nameof(count),	$"count 必須介於 0 和 {nameof(source)} 內有的物件數之間。");

			random ??= new Random();

			T[] sourceArray = source.ToArray();
			random = random ?? new Random();

			for (int i = 0; i < count; i++)
			{
				// 隨機挑選 [i, list.Count) 區間的一個索引
				int index = random.Next(i, sourceCount);
				GenericsTools<T>.Swap(ref sourceArray[i], ref sourceArray[index]);
				yield return sourceArray[i];
			}
		}

		static IEnumerable<T> GetRandomUniqueByReservoirSampling<T>(IEnumerable<T> source, int count, Random random = null)
		{
			if (source is null)
				throw new ArgumentNullException($"{nameof(source)}是Null");
			int sourceCount = source.Count();
			if (count < 0 || count > sourceCount)
				throw new ArgumentOutOfRangeException(nameof(count), $"count 必須介於 0 和 {sourceCount} 之間。");

			random ??= new Random();

			// 1. 先把前 k 個塞進蓄水池
			T[] reservoir = new T[count];
			IEnumerator<T> e = source.GetEnumerator();
			int i = 0;
			while (i < count && e.MoveNext())
			{
				reservoir[i] = e.Current;
				i++;
			}

			// 2. 往後每讀到第 i 個元素（i 從 k+1 起）以機率 k/i 決定是否取代
			while (e.MoveNext())
			{
				i++; // 現在是第 i 個（1-based）
				int r = random.Next(i); // 0 ~ i-1
				if (r < count)
					reservoir[r] = e.Current; // 以等機率取代
			}
			T[] values = reservoir;
			values = ArrayExtension.ArrayRandom(values);
			foreach (T value in values)
			{
				yield return value;
			}
		}

        /// <summary>
        /// 從一個集合中隨機抽取 N 個的項目，取後放回。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<T> GetRandom<T>(IEnumerable<T> source, int count, Random random = null)
        {
            if (source is null)
                throw new ArgumentNullException($"{nameof(source)}是Null");
            int sourceCount = source.Count();
			if (sourceCount <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(source)}沒有物件在內");

            T[] sourceArray = source.ToArray();
            random = random ?? new Random();

            for (int i = 0; i < count; i++)
            {
                // 隨機挑選 [i, list.Count) 區間的一個索引
                int index = random.Next(0, sourceCount);
                yield return sourceArray[index];
            }
        }

		public static int GetRandomOutOfIEnumerable(IEnumerable<int> source)
		{
            int r;
            do
            {
                Random random = new Random();
                r = random.Next(0, int.MaxValue);
            } while (source.Contains(r));
			return r;
        }
    }
}
