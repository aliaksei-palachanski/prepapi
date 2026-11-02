using Microsoft.EntityFrameworkCore;
using PrepApi.Models;

namespace PrepApi.Data;

public class EfTodoRepository(AppDbContext db) : ITodoRepository
{
    public Task<List<Todo>> GetAllAsync(CancellationToken ct = default) =>
        db.Todos.AsNoTracking().ToListAsync(ct);

    public Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Todos.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<bool> ExistsWithTitleAsync(string title, int? excludeId = null, CancellationToken ct = default) =>
        db.Todos.AnyAsync(t => t.Title == title && (excludeId == null || t.Id != excludeId), ct);

    public void Add(Todo todo) => db.Todos.Add(todo);
    public void Remove(Todo todo) => db.Todos.Remove(todo);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}