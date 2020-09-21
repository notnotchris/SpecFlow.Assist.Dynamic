using System.Text.RegularExpressions;
using TechTalk.SpecFlow.Assist;

namespace SpecFlow.Assist.Dynamic.PropertyNameFormatting
{
    public sealed class DefaultPropertyNameFormatter : IPropertyNameFormatter
    {
        public string Format(string name)
        {
            var cleanedHeader = RemoveReservedChars(name);
            var propName = FixCasing(cleanedHeader);

            // Throw if no chars in string
            if (propName.Length != 0) return propName;

            var mess = $"Property '{name}' only contains reserved C# characters";
            throw new DynamicInstanceFromTableException(mess);
        }

        private static string FixCasing(string header)
        {
            var arr = header.Split(' ');

            // leave the first element as is, since it might be correctly cased...
            var propName = arr[0];

            for (var i = 1; i < arr.Length; i++)
            {
                var s = arr[i];
                if (s.Length > 0)
                {
                    propName += s[0].ToString().ToUpperInvariant()
                        + s.Substring(1).ToLowerInvariant();
                }
            }

            return propName;
        }

        private static string RemoveReservedChars(string orgPropertyName)
        {
            const string pattern = @"[^\w\s]";
            const string replacement = "";
            return Regex.Replace(orgPropertyName, pattern, replacement);
        }
    }
}
