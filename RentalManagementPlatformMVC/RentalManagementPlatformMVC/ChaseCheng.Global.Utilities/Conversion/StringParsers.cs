using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseCheng.Global.Utilities.Conversion
{
    public static class StringParsers
    {
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static int[] TryParseIntArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<int>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = int.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static uint[] TryParseUIntArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<uint>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = uint.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static long[] TryParseLongArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<long>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = long.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static ulong[] TryParseULongArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<ulong>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = ulong.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static decimal[] TryParseDecimalArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<decimal>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = decimal.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static double[] TryParseDoublelArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<double>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = double.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
        /// <summary>
        /// 嘗試將以分號（;）分隔的字串轉換為整數陣列。
        /// 會自動去除每個項目的前後空白，並忽略無法成功解析為整數的項目。
        /// </summary>
        /// <param name="str">來源字串，例如 "1; 23; 456"</param>
        /// <returns>成功解析的整數陣列，若輸入為 null 或空字串則回傳空陣列</returns>
        public static float[] TryParseFloatArray(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return Array.Empty<float>();

            return str
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    bool success = float.TryParse(s.Trim(), out var value);
                    return (success, value);
                })
                .Where(t => t.success)
                .Select(t => t.value)
                .ToArray();
        }
    }
}
