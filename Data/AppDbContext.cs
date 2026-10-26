using Microsoft.EntityFrameworkCore;
using PrepApi.Models;

namespace PrepApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();
}