using System.Windows;
using System.Windows.Controls;
using EisenhowerMatrix.ViewModels;

namespace EisenhowerMatrix.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void SetViewModel(MainViewModel vm)
    {
        DataContext = vm;

        vm.ShowEditDialog = dialogVm =>
        {
            var dialog = new TaskEditDialog
            {
                DataContext = dialogVm,
                Owner = this
            };
            return dialog.ShowDialog();
        };

        vm.ShowConfirmDialog = message =>
        {
            var result = MessageBox.Show(message, "确认删除", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        };
    }

    private void StatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo &&
            combo.DataContext is TaskItemViewModel taskVm &&
            DataContext is MainViewModel mainVm &&
            combo.SelectedValue is Models.TaskItemStatus newStatus)
        {
            mainVm.UpdateTaskStatus(taskVm, newStatus);
        }
    }
}
