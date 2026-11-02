using PrepApi.Models;

namespace PrepApi.Data;

public interface ITodoRepository
{
    Task<List<Todo>> GetAllAsync(CancellationToken ct = default);
    Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsWithTitleAsync(string title, int? excludeId = null, CancellationToken ct = default);
    void Add(Todo todo);
    void Remove(Todo todo);
    Task SaveChangesAsync(CancellationToken ct = default);
}