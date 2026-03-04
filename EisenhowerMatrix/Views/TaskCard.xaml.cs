using System.Windows.Controls;
using System.Windows.Input;
using EisenhowerMatrix.ViewModels;

namespace EisenhowerMatrix.Views;

public partial class TaskCard : UserControl
{
    public TaskCard()
    {
        InitializeComponent();
    }

    private void Card_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && DataContext is TaskItemViewModel taskVm)
        {
            taskVm.EditCommand.Execute(null);
            e.Handled = true;
        }
    }
}
