using DigitizingNoteFs.Shared.Utilities;
namespace DigitizingNoteFs.XConsole
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var str1 = "chi phi nhan cong";
            var str2 = "chi phi nhap hang";

            var similarity = StringSimilarityUtils.CalculateSimilarity(str1, str2);
            Console.WriteLine(similarity);
        }
    }


}