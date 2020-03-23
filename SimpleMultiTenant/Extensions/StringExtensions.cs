using System.Text.RegularExpressions;

namespace SimpleMultiTenant.Extensions
{
	public static class StringExtensions
	{
		public static string FromCamelToProperCase(this string str)
		{
			return Regex.Replace(str, "(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])|(?<=[^0-9])(?<x>[0-9])(?=.)", " $1");
		}

		public static string FixUrlWithSlash(this string inputString)
		{
			return inputString.Replace(@"%2F", @"/");
		}

		public static int NumberOfSlashes(this string inputString)
		{
			int count = 0;

			foreach (var character in inputString)
			{
				if (character == '/')
				{
					count++;
				}
				else if (character == '\\')
				{
					count++;
				}
			}

			return count;
		}
	}
}
