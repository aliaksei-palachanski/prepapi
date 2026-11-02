using PrepApi.Models;

namespace PrepApi.Services;

public enum TodoErrorKind { Validation, Duplicate, NotFound }

public sealed record TodoError(TodoErrorKind Kind, string Message);

public sealed record TodoResult(Todo? Todo, TodoError? Error)
{
    public bool IsSuccess => Error is null;
    public static TodoResult Ok(Todo todo) => new(todo, null);
    public static TodoResult Fail(TodoErrorKind kind, string message) => new(null, new(kind, message));
}