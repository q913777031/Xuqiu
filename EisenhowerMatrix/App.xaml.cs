using System.Windows;
using Autofac;
using EisenhowerMatrix.Data;
using EisenhowerMatrix.Services;
using EisenhowerMatrix.ViewModels;
using EisenhowerMatrix.Views;
using NLog;

namespace EisenhowerMatrix;

public partial class App : Application
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private IContainer? _container;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            // Initialize database
            var freeSql = AppDbContext.Initialize();

            // Build DI container
            var builder = new ContainerBuilder();
            builder.RegisterInstance(freeSql).As<IFreeSql>().SingleInstance();
            builder.RegisterType<TaskService>().SingleInstance();
            builder.RegisterType<MainViewModel>().SingleInstance();
            _container = builder.Build();

            // Create and show main window
            var mainVm = _container.Resolve<MainViewModel>();
            var mainWindow = new MainWindow();
            mainWindow.SetViewModel(mainVm);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Application startup failed");
            MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppDbContext.Dispose();
        _container?.Dispose();
        LogManager.Shutdown();
        base.OnExit(e);
    }
}
