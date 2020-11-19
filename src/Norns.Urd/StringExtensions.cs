namespace System
{
    public static class StringExtensions
    {
        public static unsafe bool Matches(this string input, string pattern)
        {
            bool matched = false;

            fixed (char* p_wild = pattern)
            fixed (char* p_str = input)
            {
                char* wild = p_wild, str = p_str, cp = null, mp = null;

                while ((*str) != 0 && (*wild != '*'))
                {
                    if ((*wild != *str) && (*wild != '?')) return matched; wild++; str++;
                }

                while (*str != 0)
                {
                    if (*wild == '*') { if (0 == (*++wild)) return matched; mp = wild; cp = str + 1; }
                    else if ((*wild == *str) || (*wild == '?')) { wild++; str++; } else { wild = mp; str = cp++; }
                }

                while (*wild == '*') wild++; return (*wild) == 0;
            }
        }
    }
}