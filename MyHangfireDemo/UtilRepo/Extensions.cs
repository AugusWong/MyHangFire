using System;
using System.Collections.Generic;
using System.Text;

namespace UtilRepo
{
    public static class Extensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }
    }
}
