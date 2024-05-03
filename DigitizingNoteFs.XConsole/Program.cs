using DigitizingNoteFs.Shared.Utilities;
namespace DigitizingNoteFs.XConsole
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
           var str1 = "so du dau ky";
           var str2 = "so dau ";

            var similarity = StringSimilarityUtils.CalculateSimilarity(str1, str2);
            Console.WriteLine(similarity);

        }
    }

   
}