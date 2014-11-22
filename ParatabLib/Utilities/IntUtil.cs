using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParatabLib.Utilities
{
    public class IntUtil
    {
        public static bool isEqual(int? num1, int num2)
        {
            if (num1 != null)
                return num1.Value == num2;
            else
                return false;
        }

        public static bool isEqual(int num1, int? num2)
        {
            if (num2 != null)
                return num1 == num2.Value;
            else
                return false;
        }
    }
}