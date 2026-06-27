var builder = DistributedApplication.CreateBuilder(args);

var sqlDb = builder.AddSqlServer(
        "sql-server",
        builder.AddParameter("sql-server-password", secret: true))
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("Todos");

var todoapi = builder.AddProject<Projects.Todo_Api>("todoapi")
    .WaitFor(sqlDb)
    .WithReference(sqlDb);

builder.AddProject<Projects.Todo_Web_Server>("todo-web-server")
    .WaitFor(todoapi)
    .WithReference(todoapi);

builder.Build().Run();
