using System.Text;
using System.Text.RegularExpressions;

namespace DigitizingNoteFs.Shared.Utilities
{
    public static partial class StringUtils
    {
        public const string MoneyStringPattern = @"\b(\d{1,3}(?:[.,]\d{3})*(?:(?:[.,]\d{1,3})|\b))\b|\((\d{1,3}(?:[.,]\d{3})*(?:(?:[.,]\d{1,3})|\b))\)";
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


        public static List<List<string>> ConvertToMatrix(string data,
            StringSplitOptions rowOption = StringSplitOptions.RemoveEmptyEntries, StringSplitOptions colOption = StringSplitOptions.None)
        {
            string[] lines = data.Split(new[] { Environment.NewLine }, rowOption);
            List<List<string>> matrix = [];

            foreach (var line in lines)
            {
                List<string> row = new(line.Split(new[] { '\t' }, colOption));
                matrix.Add(row);
            }
            return matrix;
        }
    }
}
