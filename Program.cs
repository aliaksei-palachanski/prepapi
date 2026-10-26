using Microsoft.EntityFrameworkCore;
using PrepApi.Data;
using PrepApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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


var todos = app.MapGroup("/todos");

todos.MapGet("/", async (AppDbContext db) =>
    await db.Todos.ToListAsync());

todos.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Todos.FindAsync(id) is Todo t ? Results.Ok(t) : Results.NotFound());

todos.MapPost("/", async (Todo todo, AppDbContext db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

todos.MapPut("/{id:int}", async (int id, Todo input, AppDbContext db) =>
{
    var t = await db.Todos.FindAsync(id);
    if (t is null) return Results.NotFound();
    t.Title = input.Title;
    t.IsDone = input.IsDone;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

todos.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var t = await db.Todos.FindAsync(id);
    if (t is null) return Results.NotFound();
    db.Todos.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
