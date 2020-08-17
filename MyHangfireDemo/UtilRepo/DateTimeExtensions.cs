using System;
using System.Collections.Generic;
using System.Text;

namespace UtilRepo
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this string str)
        {
            var dt = DateTime.Now;
            if (DateTime.TryParse(str, out dt))
                return dt;
            throw new Exception("日期格式不正确");
        }
    }
}
