using System.Collections.Generic;
using System.Linq;

namespace Domain.Tenants.Multitenancy
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a list of strings from a comma delimited string and remove trailing whitespaces and
        /// turns it to lower invariant.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static List<string> CommaDelimitedStringToList(this string inputString)
        {
            return inputString.Split(",").Select(word => word.Trim().ToLowerInvariant()).ToList();
        }
    }
}
