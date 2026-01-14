var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DefenseTower>("defensetower");

builder.Build().Run();
