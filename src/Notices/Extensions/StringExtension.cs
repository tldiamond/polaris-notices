using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Extensions
{
    public static class StringExtension
    {
        public static string Pluralize(this string input, int count)
        {
            return string.Format("{0}{1}", input, count == 1 ? "" : "s");
        }
    }
}