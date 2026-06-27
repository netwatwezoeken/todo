namespace Todo.Api;

public static class MigrationManager
{
    extension(IHost source)
    {
        public IHost Migrate()
        {
            using var scope = source.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            db.Database.EnsureCreated();

            return source;
        }
    }
}