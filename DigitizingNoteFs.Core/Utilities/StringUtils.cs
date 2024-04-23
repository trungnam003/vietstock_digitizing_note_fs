using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DigitizingNoteFs.Core.Utilities
{
    public static partial class StringUtils
    {
        public static string RemoveSign4VietnameseString(this string s)
        {
            Regex regex = IsCombiningDiacriticalMarksRegex();
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string RemoveSpecialCharacters(this string s)
        {
            return NormalCharacterRegex().Replace(s, "");
        }

        [GeneratedRegex("\\p{IsCombiningDiacriticalMarks}+")]
        private static partial Regex IsCombiningDiacriticalMarksRegex();
        [GeneratedRegex("[^a-zA-Z0-9_.\\s]+", RegexOptions.Compiled)]
        private static partial Regex NormalCharacterRegex();
    }
}
