using System;
using System.Collections.Generic;
using System.Text;

namespace UtilRepo
{
    public static class EnumExtensions
    {
        public static int ToInt(this Enum @enum)
        {
            return @enum.GetHashCode();
        }
    }
}
