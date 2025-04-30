namespace CrossEngine.Utils.Extensions
{
    public static class StringExtension
    {
        public static string RemoveSuffix(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }

            return s;
        }

        public static string RemovePrefix(this string s, string prefix)
        {
            if (s.StartsWith(prefix))
            {
                return s.Substring(prefix.Length, s.Length - prefix.Length);
            }

            return s;
        }
    }
}
