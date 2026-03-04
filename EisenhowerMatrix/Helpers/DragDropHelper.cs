using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EisenhowerMatrix.Models;
using EisenhowerMatrix.ViewModels;

namespace EisenhowerMatrix.Helpers;

public static class DragDropHelper
{
    private static Point _startPoint;
    private static bool _isDragging;

    #region IsDragSource Attached Property

    public static readonly DependencyProperty IsDragSourceProperty =
        DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDragSourceChanged));

    public static bool GetIsDragSource(DependencyObject obj) => (bool)obj.GetValue(IsDragSourceProperty);
    public static void SetIsDragSource(DependencyObject obj, bool value) => obj.SetValue(IsDragSourceProperty, value);

    private static void OnIsDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if ((bool)e.NewValue)
        {
            element.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
            element.PreviewMouseMove += DragSource_PreviewMouseMove;
        }
        else
        {
            element.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
            element.PreviewMouseMove -= DragSource_PreviewMouseMove;
        }
    }

    private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Don't start drag if clicking a button
        if (e.OriginalSource is DependencyObject source &&
            FindVisualParent<Button>(source) != null)
            return;

        _startPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var currentPoint = e.GetPosition(null);
        var diff = currentPoint - _startPoint;

        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        if (_isDragging) return;
        _isDragging = true;

        if (sender is FrameworkElement element && element.DataContext is TaskItemViewModel taskVm)
        {
            var data = new DataObject("TaskItem", taskVm);
            DragDrop.DoDragDrop(element, data, DragDropEffects.Move);
        }

        _isDragging = false;
    }

    #endregion

    #region IsDropTarget Attached Property

    public static readonly DependencyProperty IsDropTargetProperty =
        DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDropTargetChanged));

    public static bool GetIsDropTarget(DependencyObject obj) => (bool)obj.GetValue(IsDropTargetProperty);
    public static void SetIsDropTarget(DependencyObject obj, bool value) => obj.SetValue(IsDropTargetProperty, value);

    private static void OnIsDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if ((bool)e.NewValue)
        {
            element.AllowDrop = true;
            element.DragOver += DropTarget_DragOver;
            element.DragLeave += DropTarget_DragLeave;
            element.Drop += DropTarget_Drop;
        }
        else
        {
            element.AllowDrop = false;
            element.DragOver -= DropTarget_DragOver;
            element.DragLeave -= DropTarget_DragLeave;
            element.Drop -= DropTarget_Drop;
        }
    }

    private static void DropTarget_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("TaskItem"))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.Move;

        if (sender is FrameworkElement element)
        {
            var border = FindVisualChild<Border>(element, "DropHighlightBorder");
            if (border != null)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
                border.BorderThickness = new Thickness(2);
            }
        }

        e.Handled = true;
    }

    private static void DropTarget_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            var border = FindVisualChild<Border>(element, "DropHighlightBorder");
            if (border != null)
            {
                border.BorderBrush = Brushes.Transparent;
                border.BorderThickness = new Thickness(0);
            }
        }
    }

    private static void DropTarget_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("TaskItem")) return;

        if (sender is FrameworkElement element)
        {
            var border = FindVisualChild<Border>(element, "DropHighlightBorder");
            if (border != null)
            {
                border.BorderBrush = Brushes.Transparent;
                border.BorderThickness = new Thickness(0);
            }
        }

        // DropQuadrant is set on QuadrantPanel
        var targetQuadrant = GetDropQuadrant(sender as DependencyObject);
        var taskVm = e.Data.GetData("TaskItem") as TaskItemViewModel;

        if (taskVm == null) return;

        // Find MainViewModel through visual tree
        var window = Window.GetWindow(sender as DependencyObject);
        if (window?.DataContext is MainViewModel mainVm)
        {
            mainVm.MoveTask(taskVm, targetQuadrant);
        }

        e.Handled = true;
    }

    #endregion

    #region DropQuadrant Attached Property

    public static readonly DependencyProperty DropQuadrantProperty =
        DependencyProperty.RegisterAttached("DropQuadrant", typeof(QuadrantType), typeof(DragDropHelper),
            new PropertyMetadata(QuadrantType.Q1));

    public static QuadrantType GetDropQuadrant(DependencyObject? obj) =>
        obj != null ? (QuadrantType)obj.GetValue(DropQuadrantProperty) : QuadrantType.Q1;

    public static void SetDropQuadrant(DependencyObject obj, QuadrantType value) =>
        obj.SetValue(DropQuadrantProperty, value);

    #endregion

    #region Visual Tree Helpers

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T result) return result;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T element && element.Name == name)
                return element;

            var result = FindVisualChild<T>(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    #endregion
}
