using EisenhowerMatrix.Models;

namespace EisenhowerMatrix.Services;

public class BoardService
{
    private readonly IFreeSql _freeSql;

    public BoardService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public List<Board> GetAllBoards()
    {
        return _freeSql.Select<Board>()
            .Where(b => b.IsArchived == 0)
            .OrderBy(b => b.SortOrder)
            .ToList();
    }

    public Board? GetBoardById(int id)
    {
        return _freeSql.Select<Board>().Where(b => b.Id == id).First();
    }

    public Board AddBoard(Board board)
    {
        board.CreatedAt = DateTime.Now;
        var maxSort = _freeSql.Select<Board>().Max(b => b.SortOrder);
        board.SortOrder = maxSort + 1;
        _freeSql.Insert(board).ExecuteAffrows();
        return _freeSql.Select<Board>().OrderByDescending(b => b.Id).First();
    }

    public void UpdateBoard(Board board)
    {
        _freeSql.Update<Board>().SetSource(board).ExecuteAffrows();
    }

    public void DeleteBoard(int id)
    {
        if (id == 1) return; // Cannot delete default board
        // Move tasks to default board
        _freeSql.Update<TaskItem>().Set(t => t.BoardId, 1).Where(t => t.BoardId == id).ExecuteAffrows();
        _freeSql.Delete<Board>().Where(b => b.Id == id).ExecuteAffrows();
    }

    public void ArchiveBoard(int id)
    {
        if (id == 1) return;
        _freeSql.Update<Board>().Set(b => b.IsArchived, 1).Where(b => b.Id == id).ExecuteAffrows();
    }
}
