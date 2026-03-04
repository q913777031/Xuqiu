using System.Windows;
using EisenhowerMatrix.ViewModels;

namespace EisenhowerMatrix.Views;

public partial class TaskEditDialog : Window
{
    public TaskEditDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is TaskEditDialogViewModel vm)
        {
            vm.SaveCommand.Execute(null);
            if (vm.DialogResult == true)
            {
                DialogResult = true;
                Close();
            }
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
