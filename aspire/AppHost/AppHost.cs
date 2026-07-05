var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ReceiptsApp_Api>("api");

builder.Build().Run();
