using System.Configuration;
using System.Data;
using System.Windows;
using HackyStaty03.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HackyStaty03
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.ConfigureServices();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

        }

    }

    public static class ServiceCollectionExtensions
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

        }

    }

}
