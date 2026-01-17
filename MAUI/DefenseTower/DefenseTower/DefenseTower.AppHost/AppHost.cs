var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DefenseTowerOrigin>("defensetower");

builder.Build().Run();
