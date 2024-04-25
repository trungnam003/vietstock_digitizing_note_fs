namespace DigitizingNoteFs.XConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Tạo các task
            Task<int> task1 = Task.Run(async () =>
            {
                Console.WriteLine("Task 1 is running");
                await Task.Delay(3000); // Task 1 chờ 3 giây
                // Thêm logic xử lý của Task 1 ở đây
                return 10; // Task 1 trả về một số nguyên
            });

            Task<string> task2 = Task.Run(async () =>
            {
                Console.WriteLine("Task 2 is running");
                await Task.Delay(2000); // Task 2 chờ 2 giây
                // Thêm logic xử lý của Task 2 ở đây
                return "Hello"; // Task 2 trả về một chuỗi
            });

            Task<bool> task3 = Task.Run(async () =>
            {
                Console.WriteLine("Task 3 is running");
                // Thêm logic xử lý của Task 3 ở đây
                await Task.Delay(2000); // Task 3 chờ 1 giây
                return true; // Task 3 trả về một giá trị boolean
            });

            // Chạy tất cả các task song song và chờ cho tất cả chúng hoàn thành
            await Task.WhenAll(task1, task2, task3);

            // Lấy kết quả của từng task
            int result1 = task1.Result;
            string result2 = task2.Result;
            bool result3 = task3.Result;

            // In kết quả
            Console.WriteLine("Result of Task 1: " + result1);
            Console.WriteLine("Result of Task 2: " + result2);
            Console.WriteLine("Result of Task 3: " + result3);
        }
    }
}