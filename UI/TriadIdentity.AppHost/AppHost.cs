var builder = DistributedApplication.CreateBuilder(args);

var jwtKey = builder.AddParameter("jwt-key", secret: true);

var db = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("DefaultConnection", "TriadIdentity");

builder.AddProject<Projects.TriadIdentity_UI_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Jwt__Key", jwtKey);

builder.Build().Run();