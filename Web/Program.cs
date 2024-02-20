using Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

app.AddMiddleware(app.Environment);

app.Run();