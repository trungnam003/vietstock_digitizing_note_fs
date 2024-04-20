using DigitizingNoteFs.Domain.IServices;


namespace DigitizingNoteFs.Infrastructure.Services
{
    public class TestService : ITestService
    {
        public string Test()
        {
            return "Hello from TestService";
        }
    }
}
