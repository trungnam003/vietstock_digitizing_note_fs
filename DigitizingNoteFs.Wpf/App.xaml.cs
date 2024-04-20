using DigitizingNoteFs.Domain.IServices;
using DigitizingNoteFs.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DigitizingNoteFs.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider? ServiceProvider { get; private set; }
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void OnStartupApplication(object sender, StartupEventArgs e)
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not initialized.");
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddTransient<ITestService, TestService>();
        }
    }
}