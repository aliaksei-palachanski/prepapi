using PrepApi.Data;
using PrepApi.Models;

namespace PrepApi.Services;

public class TodoService(ITodoRepository repo)
{
    public Task<List<Todo>> GetAllAsync(CancellationToken ct = default) => repo.GetAllAsync(ct);

    public Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default) => repo.GetByIdAsync(id, ct);

    public async Task<TodoResult> CreateAsync(string? title, CancellationToken ct = default)
    {
        var errors = TodoValidator.Validate(title);
        if (errors.Count > 0)
            return TodoResult.Fail(TodoErrorKind.Validation, string.Join(" ", errors));

        var normalized = TodoValidator.NormalizeTitle(title!);
        if (await repo.ExistsWithTitleAsync(normalized, ct: ct))
            return TodoResult.Fail(TodoErrorKind.Duplicate, $"Todo '{normalized}' already exists.");

        var todo = new Todo { Title = normalized };
        repo.Add(todo);
        await repo.SaveChangesAsync(ct);
        return TodoResult.Ok(todo);
    }

    public async Task<TodoResult> UpdateAsync(int id, string? title, bool isDone, CancellationToken ct = default)
    {
        var errors = TodoValidator.Validate(title);
        if (errors.Count > 0)
            return TodoResult.Fail(TodoErrorKind.Validation, string.Join(" ", errors));

        var todo = await repo.GetByIdAsync(id, ct);
        if (todo is null)
            return TodoResult.Fail(TodoErrorKind.NotFound, $"Todo {id} not found.");

        var normalized = TodoValidator.NormalizeTitle(title!);
        if (await repo.ExistsWithTitleAsync(normalized, id, ct))
            return TodoResult.Fail(TodoErrorKind.Duplicate, $"Todo '{normalized}' already exists.");

        todo.Title = normalized;
        todo.IsDone = isDone;
        await repo.SaveChangesAsync(ct);
        return TodoResult.Ok(todo);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var todo = await repo.GetByIdAsync(id, ct);
        if (todo is null) return false;

        repo.Remove(todo);
        await repo.SaveChangesAsync(ct);
        return true;
    }
}