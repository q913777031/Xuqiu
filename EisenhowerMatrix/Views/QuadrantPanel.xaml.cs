using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EisenhowerMatrix.Helpers;
using EisenhowerMatrix.Models;
using EisenhowerMatrix.ViewModels;

namespace EisenhowerMatrix.Views;

public partial class QuadrantPanel : UserControl
{
    public static readonly DependencyProperty QuadrantProperty =
        DependencyProperty.Register(nameof(Quadrant), typeof(QuadrantType), typeof(QuadrantPanel),
            new PropertyMetadata(QuadrantType.Q1, OnQuadrantChanged));

    public static readonly DependencyProperty TasksSourceProperty =
        DependencyProperty.Register(nameof(TasksSource), typeof(IEnumerable), typeof(QuadrantPanel),
            new PropertyMetadata(null, OnTasksSourceChanged));

    public static readonly DependencyProperty TaskCountProperty =
        DependencyProperty.Register(nameof(TaskCount), typeof(int), typeof(QuadrantPanel),
            new PropertyMetadata(0, OnTaskCountChanged));

    public QuadrantType Quadrant
    {
        get => (QuadrantType)GetValue(QuadrantProperty);
        set => SetValue(QuadrantProperty, value);
    }

    public IEnumerable TasksSource
    {
        get => (IEnumerable)GetValue(TasksSourceProperty);
        set => SetValue(TasksSourceProperty, value);
    }

    public int TaskCount
    {
        get => (int)GetValue(TaskCountProperty);
        set => SetValue(TaskCountProperty, value);
    }

    public QuadrantPanel()
    {
        InitializeComponent();
    }

    private static void OnQuadrantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuadrantPanel panel)
        {
            panel.UpdateQuadrantDisplay();
            DragDropHelper.SetDropQuadrant(panel, panel.Quadrant);
        }
    }

    private static void OnTasksSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuadrantPanel panel)
            panel.TaskList.ItemsSource = e.NewValue as IEnumerable;
    }

    private static void OnTaskCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuadrantPanel panel)
            panel.TaskCountBadge.Count = (int)(e.NewValue ?? 0);
    }

    private void UpdateQuadrantDisplay()
    {
        var (title, hint, brush) = Quadrant switch
        {
            QuadrantType.Q1 => ("Q1 重要 & 紧急", "立即做", new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))),
            QuadrantType.Q2 => ("Q2 重要 & 不紧急", "安排时间", new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16))),
            QuadrantType.Q3 => ("Q3 不重要 & 紧急", "委派他人", new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6))),
            QuadrantType.Q4 => ("Q4 不重要 & 不紧急", "有空再做", new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF))),
            _ => ("", "", new SolidColorBrush(Colors.Gray))
        };

        QuadrantTitle.Text = title;
        ActionHintText.Text = hint;
        QuadrantColorBar.Background = brush;
        DragDropHelper.SetDropQuadrant(this, Quadrant);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window?.DataContext is MainViewModel mainVm)
        {
            mainVm.AddTaskCommand.Execute(Quadrant);
        }
    }
}
