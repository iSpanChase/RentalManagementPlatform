using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Extension
{
	public static class MathExtension
	{
		#region//向上取整
		/// <summary>
		/// 向上取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double RoundUp(double value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.ToPositiveInfinity);
		}
		/// <summary>
		/// 向上取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double RoundUp(double value)
		{
			return Math.Round(value, MidpointRounding.ToPositiveInfinity);
		}
		/// <summary>
		/// 向上取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal RoundUp(decimal value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.ToPositiveInfinity);
		}
		/// <summary>
		/// 向上取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal RoundUp(decimal value)
		{
			return Math.Round(value, MidpointRounding.ToPositiveInfinity);
		}
		#endregion

		#region//4捨5入
		/// <summary>
		/// 4捨5入
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double Round(double value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.AwayFromZero);
		}
		/// <summary>
		/// 4捨5入
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double Round(double value)
		{
			return Math.Round(value, MidpointRounding.AwayFromZero);
		}
		/// <summary>
		/// 4捨5入
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal Round(decimal value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.AwayFromZero);
		}
		/// <summary>
		/// 4捨5入
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal Round(decimal value)
		{
			return Math.Round(value, MidpointRounding.AwayFromZero);
		}
		#endregion

		#region//向下取整
		/// <summary>
		/// 向下取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double RoundDown(double value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.ToNegativeInfinity);
		}
		/// <summary>
		/// 向下取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static double RoundDown(double value)
		{
			return Math.Round(value, MidpointRounding.ToNegativeInfinity);
		}
		/// <summary>
		/// 向下取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal RoundDown(decimal value, int digits)
		{
			return Math.Round(value, digits, MidpointRounding.ToNegativeInfinity);
		}
		/// <summary>
		/// 向下取整
		/// </summary>
		/// <param name="value">要計算的值</param>
		/// <param name="digits">要計算位數</param>
		/// <returns></returns>
		public static decimal RoundDown(decimal value)
		{
			return Math.Round(value, MidpointRounding.ToNegativeInfinity);
		}
		#endregion

		#region //算平方
		/// <summary>
		/// 算平方
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static double Pow2(double value)
		{
			return Math.Pow(value,2);
		}
		#endregion
	}
}
