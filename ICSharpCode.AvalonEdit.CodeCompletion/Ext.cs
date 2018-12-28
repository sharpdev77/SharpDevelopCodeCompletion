using System;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public static class Ext
    {
        /// <summary>
        /// Removes <paramref name="stringToRemove" /> from the end of this string.
        /// Throws ArgumentException if this string does not end with <paramref name="stringToRemove" />.
        /// </summary>
        public static string RemoveFromEnd(this string s, string stringToRemove)
        {
            if (s == null) return null;
            if (string.IsNullOrEmpty(stringToRemove))
                return s;
            if (!s.EndsWith(stringToRemove))
                throw new ArgumentException(string.Format("{0} does not end with {1}", s, stringToRemove));
            return s.Substring(0, s.Length - stringToRemove.Length);
        }
    }
}