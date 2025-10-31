using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	internal static class ArrayExtension
	{
		/// <summary>
		/// 以洗牌法打亂Array順序
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="random"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static T[] ArrayRandom<T>(T[] source, Random random = null)
		{
			if (source is null)
				throw new ArgumentNullException($"{nameof(source)}是Null");

			random ??= new Random();

			for (int i = 0; i < source.Length; i++)
			{
				GenericsTools<T>.Swap(ref source[i], ref source[random.Next(i, source.Length)]);
			}

			return source;
		}

	}
}
