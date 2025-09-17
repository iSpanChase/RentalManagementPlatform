namespace RentalManagementPlatformMVC.Areas.ReportForm.Helpers
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// 返回時間是否介於區間內，包含小的，不包含大的 date1 <= input < date2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static bool Between(this DateTime input, DateTime date1, DateTime date2)
        {
            if(date1> date2)
            {
                var temp = date1;
                date1 = date2;
                date2 = temp;
            }
            return (input >= date1 && input < date2);
        }
    }
}
