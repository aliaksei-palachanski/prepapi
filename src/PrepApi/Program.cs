using Microsoft.EntityFrameworkCore;
using PrepApi.Data;
using PrepApi.Models;
using PrepApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ITodoRepository, EfTodoRepository>();
builder.Services.AddScoped<TodoService>();
builder.Services.AddHealthChecks();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //   app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "PrepApi"));
}

app.UseHttpsRedirection();
 
// Применяем миграции при старте
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.MapHealthChecks("/health");

var todos = app.MapGroup("/todos");

todos.MapGet("/", (TodoService s, CancellationToken ct) => s.GetAllAsync(ct));

todos.MapGet("/{id:int}", async (int id, TodoService s, CancellationToken ct) =>
    await s.GetByIdAsync(id, ct) is Todo t ? Results.Ok(t) : Results.NotFound());

todos.MapPost("/", async (TodoRequest req, TodoService s, CancellationToken ct) =>
{
    var r = await s.CreateAsync(req.Title, ct);
    return r.IsSuccess ? Results.Created($"/todos/{r.Todo!.Id}", r.Todo) : ToProblem(r.Error!);
});

todos.MapPut("/{id:int}", async (int id, TodoRequest req, TodoService s, CancellationToken ct) =>
{
    var r = await s.UpdateAsync(id, req.Title, req.IsDone, ct);
    return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error!);
});

todos.MapDelete("/{id:int}", async (int id, TodoService s, CancellationToken ct) =>
    await s.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

static IResult ToProblem(TodoError e) => e.Kind switch
{
    TodoErrorKind.Validation => Results.ValidationProblem(
        new Dictionary<string, string[]> { ["title"] = [e.Message] }),
    TodoErrorKind.Duplicate => Results.Conflict(new { error = e.Message }),
    TodoErrorKind.NotFound => Results.NotFound(),
    _ => Results.Problem()
};


app.Run();