using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public static class StringExtensions
    {
        public static bool ContainsAny(this string source, ICollection<string> values)
        {
            foreach (var value in values)
            {
                if (source.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsAll(this string source, ICollection<string> values)
        {
            foreach (var value in values)
            {
                if (!source.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
