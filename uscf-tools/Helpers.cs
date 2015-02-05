using System.Runtime.CompilerServices;

// make it visible to the unit test project
[assembly: InternalsVisibleTo("uscf-tools.tests")]
namespace uscf_tools
{
    /// <summary>
    /// Various helper static methods, including extensions
    /// </summary>
    internal static class Helpers
    {
        /// <summary>
        /// String extension method that replaces all of the occurrences of a search string within a target with
        /// a replacement string. The original String.Replace does not capture the situation when the replacement results
        /// in another occurrence of the search string, and simply moves forward. For example when you want to replace double space with
        /// the single space, the triple space will end up being reduced to a double space, but not to a single space. 
        /// </summary>
        /// <param name="obj">string object</param>
        /// <param name="search">string to search for</param>
        /// <param name="replace">string to replace with</param>
        /// <returns></returns>
        internal static string ReplaceAll(this string obj, string search, string replace)
        {
            var result = obj;
            var lenBefore = result.Length;
            result = result.Replace(search, replace);
            var lenAfter = result.Length;

            while (lenAfter != lenBefore)
            {
                lenBefore = result.Length;
                result = result.Replace(search, replace);
                lenAfter = result.Length;
            }

            return result;
        }
    }
}
