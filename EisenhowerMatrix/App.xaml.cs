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
            var freeSql = AppDbContext.Initialize();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(freeSql).As<IFreeSql>().SingleInstance();
            builder.RegisterType<TaskService>().SingleInstance();
            builder.RegisterType<BoardService>().SingleInstance();
            builder.RegisterType<TagService>().SingleInstance();
            builder.RegisterType<PomodoroService>().SingleInstance();
            builder.RegisterType<SettingsService>().SingleInstance();
            builder.RegisterType<TemplateService>().SingleInstance();
            builder.RegisterType<UndoRedoService>().SingleInstance();
            builder.RegisterType<CsvService>().SingleInstance();
            builder.RegisterType<AnalyticsService>().SingleInstance();
            builder.RegisterType<MainViewModel>().SingleInstance();
            _container = builder.Build();

            var mainVm = _container.Resolve<MainViewModel>();
            var mainWindow = new MainWindow();
            mainWindow.SetViewModel(mainVm);

            // Apply saved theme
            var settings = _container.Resolve<SettingsService>();
            var theme = settings.Theme;
            if (theme == "Dark")
                AntDesign.WPF.ThemeHelper.SetBaseTheme(AntDesign.WPF.BaseTheme.Dark);

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
