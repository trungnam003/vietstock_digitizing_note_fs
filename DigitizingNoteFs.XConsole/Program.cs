using DigitizingNoteFs.Shared.Utilities;
namespace DigitizingNoteFs.XConsole
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            int x1 = 0, y1 = 0, x2 = 1, y2 = 1;

            var distance = CoreUtils.EuclideanDistance(x1, y1, x2, y2);
            Console.WriteLine(distance);
            var abc1 = new ABC()
            {
                Name = "abc1"
            };

        }
    }

    public  class ABC
    {
        public string Name { get; set; }
    }
}