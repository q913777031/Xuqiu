using System.Text.Json;

namespace EisenhowerMatrix.Services;

public record UndoAction(string Description, Action Undo, Action Redo);

public class UndoRedoService
{
    private readonly Stack<UndoAction> _undoStack = new();
    private readonly Stack<UndoAction> _redoStack = new();
    private const int MaxStackSize = 50;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event Action? StateChanged;

    public void Execute(UndoAction action)
    {
        action.Redo();
        _undoStack.Push(action);
        _redoStack.Clear();

        // Limit stack size
        if (_undoStack.Count > MaxStackSize)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = Math.Min(items.Length - 1, MaxStackSize - 1); i >= 0; i--)
                _undoStack.Push(items[i]);
        }

        StateChanged?.Invoke();
    }

    public void RecordAction(string description, Action undo, Action redo)
    {
        _undoStack.Push(new UndoAction(description, undo, redo));
        _redoStack.Clear();

        if (_undoStack.Count > MaxStackSize)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = Math.Min(items.Length - 1, MaxStackSize - 1); i >= 0; i--)
                _undoStack.Push(items[i]);
        }

        StateChanged?.Invoke();
    }

    public string? Undo()
    {
        if (!CanUndo) return null;
        var action = _undoStack.Pop();
        action.Undo();
        _redoStack.Push(action);
        StateChanged?.Invoke();
        return action.Description;
    }

    public string? Redo()
    {
        if (!CanRedo) return null;
        var action = _redoStack.Pop();
        action.Redo();
        _undoStack.Push(action);
        StateChanged?.Invoke();
        return action.Description;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        StateChanged?.Invoke();
    }
}
