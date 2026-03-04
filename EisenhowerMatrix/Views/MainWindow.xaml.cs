using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            var result = MessageBox.Show(message, "确认", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        };

        vm.ShowMessage = message =>
        {
            MessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        };

        // Apply initial dark mode state from settings
        if (vm.IsDarkMode)
        {
            AntDesign.WPF.ThemeHelper.SetBaseTheme(AntDesign.WPF.BaseTheme.Dark);
        }
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

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainViewModel vm)
        {
            vm.SearchCommand.Execute(SearchBox.Text);
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.SearchCommand.Execute(SearchBox.Text);
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        // Ctrl+Z: Undo
        if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
        {
            vm.UndoCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+Y: Redo
        else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
        {
            vm.RedoCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+F: Focus search
        else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            e.Handled = true;
        }
        // Ctrl+N: New task in Q1
        else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
        {
            vm.AddTaskCommand.Execute(Models.QuadrantType.Q1);
            e.Handled = true;
        }
    }
}
