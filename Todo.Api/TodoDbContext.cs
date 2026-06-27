using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : IdentityDbContext<TodoUser>(options)
{
    public DbSet<Todo.Api.Todos.Todo> Todos => Set<Todo.Api.Todos.Todo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Todo.Api.Todos.Todo>()
               .HasOne<TodoUser>()
               .WithMany()
               .HasForeignKey(t => t.OwnerId)
               .HasPrincipalKey(u => u.Id);

        base.OnModelCreating(builder);
    }
}
