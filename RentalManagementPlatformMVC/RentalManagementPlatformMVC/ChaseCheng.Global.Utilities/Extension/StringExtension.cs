namespace ChaseCheng.Global.Utilities.Extension
{
	public static class StringExtension
    {
        /// <summary>
        /// 擷取字串左側指定長度
        /// </summary>
        /// <param name="str">原始字串</param>
        /// <param name="length">擷取長度</param>
        /// <returns></returns>
        public static string Truncate(string str, int length)
        {
            if(string.IsNullOrEmpty(str))
                return string.Empty;
            if(length < 0)
                return string.Empty;
            if(str.Length <= length)
                return str;

            return str.Substring(0, length);
		}
	}
}
