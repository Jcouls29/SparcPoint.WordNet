using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparcPoint.WordNet
{
    static class ParseHelper
    {
        public static string GetNextField(string key, ref int lastIndex, char separator)
        {
            int sepIndex = key.IndexOf(separator, lastIndex + 1);
            if (sepIndex == -1) throw new ArgumentException("Invalid Sense Key Format.", nameof(key));
            string rtn = key.Substring(lastIndex + 1, sepIndex - lastIndex - 1);
            lastIndex = sepIndex;
            return rtn;
        }

        public static string GetNextField(string key, ref int lastIndex, string separator)
        {
            int sepIndex = key.IndexOf(separator, lastIndex + 1);
            if (sepIndex == -1) throw new ArgumentException("Invalid Sense Key Format.", nameof(key));
            string rtn = key.Substring(lastIndex + 1, sepIndex - lastIndex - 1);
            lastIndex = sepIndex + separator.Length - 1;
            return rtn;
        }

        public static bool NextSeparatorExists(string key, int lastIndex, char separator)
        {
            int sepIndex = key.IndexOf(separator, lastIndex + 1);
            return (sepIndex > -1);
        }

        public static bool NextSeparatorExists(string key, int lastIndex, string separator)
        {
            int sepIndex = key.IndexOf(separator, lastIndex + 1);
            return (sepIndex > -1);
        }

        public static string GetLastField(string key, int lastIndex)
        {
            return key.Substring(lastIndex + 1);
        }

        public static bool IsNumber(char c)
        {
            return char.IsNumber(c);
        }
    }
}
